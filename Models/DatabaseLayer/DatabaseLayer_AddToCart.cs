using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string? ipAddress, int productId);
        Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change);
        Task MergeGuestCart(int userId, string ipAddress);
        Task<IActionResult> DeleteCartItem(int id);
        Task<IActionResult> ClearCart(int? userId, string? ipAddress);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        // =========================
        // ✅ GET CART
        // =========================
        public async Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress)
        {
            var cartList = new List<CartItemModel>();

            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
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
                    p.productname,
                    p.slug,
                    p.image,
                    p.price,
                    p.discountprice,
                    c.quantity,
                    (c.quantity * COALESCE(p.discountprice, p.price)) AS totalprice,
                    c.createdat
                FROM addtocart c
                INNER JOIN product p ON p.id = c.productid
                {whereClause}
                ORDER BY c.createdat DESC;
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (userId.HasValue)
                        command.Parameters.AddWithValue("@UserId", userId.Value);
                    else if (!string.IsNullOrEmpty(ipAddress))
                        command.Parameters.AddWithValue("@IpAddress", ipAddress);
                    else
                        return cartList;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cartList.Add(new CartItemModel
                            {
                                id = reader.GetInt32("id"),
                                userid = reader.IsDBNull("userid") ? null : reader.GetInt32("userid"),
                                ipaddress = reader.IsDBNull("ipaddress") ? null : reader.GetString("ipaddress"),
                                productid = reader.GetInt32("productid"),
                                ProductName = reader.GetString("productname"),
                                Slug = reader.IsDBNull("slug") ? null : reader.GetString("slug"),
                                Image = reader.IsDBNull("image") ? null : reader.GetString("image"),
                                Price = reader.GetDecimal("price"),
                                DiscountPrice = reader.IsDBNull("discountprice") ? null : reader.GetDecimal("discountprice"),
                                quantity = reader.GetInt32("quantity"),
                                totalprice = reader.GetDecimal("totalprice"),
                                createdat = reader.IsDBNull("createdat") ? null : reader.GetDateTime("createdat")
                            });
                        }
                    }
                }
            }

            return cartList;
        }

        // =========================
        // ✅ ADD TO CART
        // =========================
        public async Task<string> AddToCart(int? userId, string? ipAddress, int productId)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                string checkQuery = userId.HasValue
                    ? "SELECT COUNT(1) FROM addtocart WHERE userid = @UserId AND productid = @ProductId"
                    : "SELECT COUNT(1) FROM addtocart WHERE ipaddress = @IpAddress AND productid = @ProductId";

                using (var checkCmd = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@ProductId", productId);

                    if (userId.HasValue)
                        checkCmd.Parameters.AddWithValue("@UserId", userId.Value);
                    else
                        checkCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                    var count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0);
                    if (count > 0)
                        return "AlreadyInCart";
                }

                string insertQuery = userId.HasValue ? @"
                    INSERT INTO addtocart (userid, productid, quantity, price)
                    SELECT @UserId, @ProductId, 1, COALESCE(p.discountprice, p.price)
                    FROM product p WHERE p.id = @ProductId;
                " : @"
                    INSERT INTO addtocart (ipaddress, productid, quantity, price)
                    SELECT @IpAddress, @ProductId, 1, COALESCE(p.discountprice, p.price)
                    FROM product p WHERE p.id = @ProductId;
                ";

                using (var insertCmd = new NpgsqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@ProductId", productId);

                    if (userId.HasValue)
                        insertCmd.Parameters.AddWithValue("@UserId", userId.Value);
                    else
                        insertCmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            return "Success";
        }

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        public async Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                string whereClause = userId.HasValue
                    ? "userid = @UserId AND productid = @ProductId"
                    : "ipaddress = @IpAddress AND productid = @ProductId";

                string query = $@"
                WITH updated AS (
                    UPDATE addtocart
                    SET quantity = quantity + @Change
                    WHERE {whereClause}
                    RETURNING id, quantity
                )
                DELETE FROM addtocart
                WHERE id IN (
                    SELECT id FROM updated WHERE quantity <= 0
                );
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Change", change);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    if (userId.HasValue)
                        command.Parameters.AddWithValue("@UserId", userId.Value);
                    else
                        command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                    await command.ExecuteNonQueryAsync();
                }
            }

            return "Success";
        }

        // =========================
        // ✅ MERGE IP CART → USER
        // =========================
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(@"
                    INSERT INTO addtocart (userid, productid, quantity, price)
                    SELECT @UserId, productid, quantity, price
                    FROM addtocart
                    WHERE ipaddress = @IpAddress
                    ON CONFLICT (userid, productid)
                    DO UPDATE SET quantity = addtocart.quantity + EXCLUDED.quantity;

                    DELETE FROM addtocart WHERE ipaddress = @IpAddress;
                ", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@IpAddress", ipAddress);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // =========================
        // ✅ DELETE ITEM
        // =========================
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("DELETE FROM addtocart WHERE id = @Id", connection);
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

            using var command = new NpgsqlCommand($"DELETE FROM addtocart WHERE {whereClause}", connection);

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