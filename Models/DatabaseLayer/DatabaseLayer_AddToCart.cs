using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string? ipAddress, int productId, int? variantId = null);
        Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null);
        Task MergeGuestCart(int userId, string ipAddress);
        Task<IActionResult> DeleteCartItem(int id);
        Task<IActionResult> ClearCart(int? userId, string? ipAddress);
        Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null); // ✅
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        private static NpgsqlParameter NullableInt(string name, int? value)
        {
            return new NpgsqlParameter(name, NpgsqlDbType.Integer)
            {
                Value = value.HasValue ? (object)value.Value : DBNull.Value
            };
        }

        // =========================
        // ✅ GET CART
        // =========================
        public async Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress)
        {
            var cartList = new List<CartItemModel>();

            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            string whereClause = userId.HasValue
                ? "WHERE c.userid = @UserId"
                : "WHERE c.ipaddress = @IpAddress";

            string query = $@"
                SELECT
                    c.id,
                    c.userid,
                    c.ipaddress,
                    c.productid,
                    c.variantid,
                    c.quantity,
                    c.createdat,

                    p.productname,
                    p.slug,
                    p.image                                          AS productimage,
                    p.price                                          AS productprice,
                    p.discountprice                                  AS productdiscountprice,

                    v.variantname,
                    v.image                                          AS variantimage,
                    v.sizeid                                         AS variantsizeids,
                    v.colorid                                        AS variantcolorids,
                    v.price                                          AS variantprice,

                    (c.quantity * COALESCE(v.price, p.discountprice, p.price)) AS totalprice

                FROM addtocart c
                INNER JOIN product p ON p.id = c.productid
                LEFT  JOIN variant v ON v.id = c.variantid
                {whereClause}
                ORDER BY c.createdat DESC;
            ";

            using var command = new NpgsqlCommand(query, connection);

            if (userId.HasValue)
                command.Parameters.AddWithValue("@UserId", userId.Value);
            else if (!string.IsNullOrEmpty(ipAddress))
                command.Parameters.AddWithValue("@IpAddress", ipAddress);
            else
                return cartList;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cartList.Add(new CartItemModel
                {
                    id = reader.GetInt32("id"),
                    userid = reader.IsDBNull("userid") ? null : reader.GetInt32("userid"),
                    ipaddress = reader.IsDBNull("ipaddress") ? null : reader.GetString("ipaddress"),
                    productid = reader.GetInt32("productid"),
                    variantid = reader.IsDBNull("variantid") ? null : reader.GetInt32("variantid"),
                    quantity = reader.GetInt32("quantity"),
                    createdat = reader.IsDBNull("createdat") ? null : reader.GetDateTime("createdat"),

                    ProductName = reader.GetString("productname"),
                    Slug = reader.IsDBNull("slug") ? null : reader.GetString("slug"),
                    ProductImage = reader.IsDBNull("productimage") ? null : reader.GetString("productimage"),
                    ProductPrice = reader.GetDecimal("productprice"),
                    ProductDiscountPrice = reader.IsDBNull("productdiscountprice") ? null : reader.GetDecimal("productdiscountprice"),

                    VariantName = reader.IsDBNull("variantname") ? null : reader.GetString("variantname"),
                    VariantImage = reader.IsDBNull("variantimage") ? null : reader.GetString("variantimage"),
                    VariantSizeIds = reader.IsDBNull("variantsizeids") ? null : (int[])reader.GetValue("variantsizeids"),
                    VariantColorIds = reader.IsDBNull("variantcolorids") ? null : (int[])reader.GetValue("variantcolorids"),
                    VariantPrice = reader.IsDBNull("variantprice") ? null : reader.GetDecimal("variantprice"),

                    totalprice = reader.GetDecimal("totalprice"),
                });
            }

            return cartList;
        }

        // =========================
        // ✅ ADD TO CART
        // =========================
        public async Task<string> AddToCart(int? userId, string? ipAddress, int productId, int? variantId = null)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            string checkQuery = userId.HasValue
                ? @"SELECT COUNT(1) FROM addtocart
                    WHERE userid    = @UserId
                      AND productid = @ProductId
                      AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))"
                : @"SELECT COUNT(1) FROM addtocart
                    WHERE ipaddress = @IpAddress
                      AND productid = @ProductId
                      AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))";

            using (var checkCmd = new NpgsqlCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@ProductId", productId);
                checkCmd.Parameters.Add(NullableInt("@VariantId", variantId));

                if (userId.HasValue)
                    checkCmd.Parameters.AddWithValue("@UserId", userId.Value);
                else
                    checkCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                var count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0);
                if (count > 0) return "AlreadyInCart";
            }

            string insertQuery = userId.HasValue
                ? @"INSERT INTO addtocart (userid, productid, variantid, quantity, price)
                    SELECT @UserId, @ProductId, @VariantId, 1,
                        CASE
                            WHEN @VariantId IS NOT NULL
                                THEN (SELECT price FROM variant WHERE id = @VariantId)
                            ELSE COALESCE(p.discountprice, p.price)
                        END
                    FROM product p WHERE p.id = @ProductId;"
                : @"INSERT INTO addtocart (ipaddress, productid, variantid, quantity, price)
                    SELECT @IpAddress, @ProductId, @VariantId, 1,
                        CASE
                            WHEN @VariantId IS NOT NULL
                                THEN (SELECT price FROM variant WHERE id = @VariantId)
                            ELSE COALESCE(p.discountprice, p.price)
                        END
                    FROM product p WHERE p.id = @ProductId;";

            using var insertCmd = new NpgsqlCommand(insertQuery, connection);
            insertCmd.Parameters.AddWithValue("@ProductId", productId);
            insertCmd.Parameters.Add(NullableInt("@VariantId", variantId));

            if (userId.HasValue)
                insertCmd.Parameters.AddWithValue("@UserId", userId.Value);
            else
                insertCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

            await insertCmd.ExecuteNonQueryAsync();
            return "Success";
        }

        // =========================
        // ✅ ADD MULTIPLE
        // Case 1: variantId null  → har product ka auto first variant pick
        // Case 2: variantId given → verify karo product ka hai, tab use karo
        // =========================
        public async Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            foreach (var productId in productIds)
            {
                int? resolvedVariantId;

                if (variantId.HasValue)
                {
                    // ✅ Case 2: variantId diya — verify karo ye is product ka hai
                    using var verifyCmd = new NpgsqlCommand(@"
                        SELECT id FROM variant
                        WHERE id = @VariantId AND productid = @ProductId AND isactive = TRUE
                        LIMIT 1", connection);

                    verifyCmd.Parameters.AddWithValue("@VariantId", variantId.Value);
                    verifyCmd.Parameters.AddWithValue("@ProductId", productId);

                    var verified = await verifyCmd.ExecuteScalarAsync();
                    resolvedVariantId = (verified != null && verified != DBNull.Value)
                        ? variantId.Value   // valid → use karo
                        : null;             // invalid → base price
                }
                else
                {
                    // ✅ Case 1: variantId nahi → auto first active variant
                    using var varCmd = new NpgsqlCommand(@"
                        SELECT id FROM variant
                        WHERE productid = @ProductId AND isactive = TRUE
                        ORDER BY id ASC LIMIT 1", connection);

                    varCmd.Parameters.AddWithValue("@ProductId", productId);
                    var result = await varCmd.ExecuteScalarAsync();

                    resolvedVariantId = (result != null && result != DBNull.Value)
                        ? Convert.ToInt32(result)
                        : null;
                }

                // NULL-safe duplicate check
                string checkQuery = userId.HasValue
                    ? @"SELECT COUNT(1) FROM addtocart
                        WHERE userid    = @UserId
                          AND productid = @ProductId
                          AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))"
                    : @"SELECT COUNT(1) FROM addtocart
                        WHERE ipaddress = @IpAddress
                          AND productid = @ProductId
                          AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))";

                using (var checkCmd = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@ProductId", productId);
                    checkCmd.Parameters.Add(NullableInt("@VariantId", resolvedVariantId));

                    if (userId.HasValue)
                        checkCmd.Parameters.AddWithValue("@UserId", userId.Value);
                    else
                        checkCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                    var count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0);
                    if (count > 0) continue;
                }

                // Insert
                string insertQuery = userId.HasValue
                    ? @"INSERT INTO addtocart (userid, productid, variantid, quantity, price)
                        SELECT @UserId, @ProductId, @VariantId, 1,
                            CASE
                                WHEN @VariantId IS NOT NULL
                                    THEN (SELECT price FROM variant WHERE id = @VariantId)
                                ELSE COALESCE(p.discountprice, p.price)
                            END
                        FROM product p WHERE p.id = @ProductId;"
                    : @"INSERT INTO addtocart (ipaddress, productid, variantid, quantity, price)
                        SELECT @IpAddress, @ProductId, @VariantId, 1,
                            CASE
                                WHEN @VariantId IS NOT NULL
                                    THEN (SELECT price FROM variant WHERE id = @VariantId)
                                ELSE COALESCE(p.discountprice, p.price)
                            END
                        FROM product p WHERE p.id = @ProductId;";

                using var insertCmd = new NpgsqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@ProductId", productId);
                insertCmd.Parameters.Add(NullableInt("@VariantId", resolvedVariantId));

                if (userId.HasValue)
                    insertCmd.Parameters.AddWithValue("@UserId", userId.Value);
                else
                    insertCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                await insertCmd.ExecuteNonQueryAsync();
            }

            return "Success";
        }

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        public async Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            string whereClause = userId.HasValue
                ? @"userid    = @UserId
                    AND productid = @ProductId
                    AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))"
                : @"ipaddress = @IpAddress
                    AND productid = @ProductId
                    AND (variantid = @VariantId OR (variantid IS NULL AND @VariantId IS NULL))";

            string query = $@"
                WITH updated AS (
                    UPDATE addtocart
                    SET quantity = quantity + @Change
                    WHERE {whereClause}
                    RETURNING id, quantity
                )
                DELETE FROM addtocart
                WHERE id IN (SELECT id FROM updated WHERE quantity <= 0);
            ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Change", change);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.Add(NullableInt("@VariantId", variantId));

            if (userId.HasValue)
                command.Parameters.AddWithValue("@UserId", userId.Value);
            else
                command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

            await command.ExecuteNonQueryAsync();
            return "Success";
        }

        // =========================
        // ✅ MERGE GUEST CART → USER
        // =========================
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                INSERT INTO addtocart (userid, productid, variantid, quantity, price)
                SELECT @UserId, productid, variantid, quantity, price
                FROM addtocart
                WHERE ipaddress = @IpAddress
                ON CONFLICT (userid, productid, COALESCE(variantid, -1))
                DO UPDATE SET quantity = addtocart.quantity + EXCLUDED.quantity;

                DELETE FROM addtocart WHERE ipaddress = @IpAddress;
            ", connection);

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@IpAddress", ipAddress);
            await command.ExecuteNonQueryAsync();
        }

        // =========================
        // ✅ DELETE ITEM
        // =========================
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "DELETE FROM addtocart WHERE id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            int rows = await command.ExecuteNonQueryAsync();

            return rows > 0
                ? new OkObjectResult(new { status = true, message = "Cart item deleted" })
                : new NotFoundObjectResult(new { status = false, message = "Cart item not found" });
        }

        // =========================
        // ✅ CLEAR CART
        // =========================
        public async Task<IActionResult> ClearCart(int? userId, string? ipAddress)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            string whereClause = userId.HasValue
                ? "userid = @UserId"
                : "ipaddress = @IpAddress";

            using var command = new NpgsqlCommand(
                $"DELETE FROM addtocart WHERE {whereClause}", connection);

            if (userId.HasValue)
                command.Parameters.AddWithValue("@UserId", userId.Value);
            else
                command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

            int rows = await command.ExecuteNonQueryAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = $"{rows} cart items cleared"
            });
        }
    }
}