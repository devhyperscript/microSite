using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;

namespace firstproject.Models.DatabaseLayer


{

    public partial interface IDatabaseLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string? ipAddress, int? productId, int? variantId = null);
        Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null);
        Task MergeGuestCart(int userId, string ipAddress);
        Task<IActionResult> DeleteCartItem(int id);
        Task<IActionResult> ClearCart(int? userId, string? ipAddress);
        Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null);
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
        // ✅ GET CART (FINAL)
        // =========================
        public async Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress)
        {
            var list = new List<CartItemModel>();

            using var con = new NpgsqlConnection(DbConnection);
            await con.OpenAsync();

            string where = userId.HasValue
                ? "c.userid = @UserId"
                : "c.ipaddress = @Ip";

            string query = $@"
SELECT
    c.id, c.userid, c.ipaddress,
    c.productid, c.variantid,
    c.quantity, c.createdat,

    CASE 
        WHEN c.variantid IS NOT NULL THEN v.variantname
        ELSE p.productname
    END AS name,

    CASE 
        WHEN c.variantid IS NOT NULL THEN v.image
        ELSE p.image
    END AS image,

    -- ✅ SALE PRICE
    CASE 
        WHEN c.variantid IS NOT NULL 
            THEN COALESCE(v.discountprice, v.price)
        ELSE 
            COALESCE(p.discountprice, p.price)
    END AS price,

    v.sizeid,
    v.colorid,

    -- ✅ TOTAL
    (c.quantity *
        CASE 
            WHEN c.variantid IS NOT NULL 
                THEN COALESCE(v.discountprice, v.price)
            ELSE 
                COALESCE(p.discountprice, p.price)
        END
    ) AS totalprice

FROM addtocart c
LEFT JOIN product p ON p.id = c.productid
LEFT JOIN variant v ON v.id = c.variantid

WHERE {where}
ORDER BY c.createdat DESC";

            using var cmd = new NpgsqlCommand(query, con);

            if (userId.HasValue)
                cmd.Parameters.Add("@UserId", NpgsqlDbType.Integer).Value = userId.Value;
            else
                cmd.Parameters.Add("@Ip", NpgsqlDbType.Text).Value = ipAddress ?? "";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new CartItemModel
                {
                    id = reader.GetInt32(0),
                    userid = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ipaddress = reader.IsDBNull(2) ? null : reader.GetString(2),

                    productid = reader.GetInt32(3),
                    variantid = reader.IsDBNull(4) ? null : reader.GetInt32(4),

                    quantity = reader.GetInt32(5),
                    createdat = reader.IsDBNull(6) ? null : reader.GetDateTime(6),

                    Name = reader.GetString(7),
                    Image = reader.IsDBNull(8) ? null : reader.GetString(8),

                    Price = reader.GetDecimal(9),

                    VariantSizeIds = reader.IsDBNull(10) ? null : (int[])reader.GetValue(10),
                    VariantColorIds = reader.IsDBNull(11) ? null : (int[])reader.GetValue(11),

                    totalprice = reader.GetDecimal(12)
                });
            }

            return list;
        }

        // =========================
        // ✅ ADD TO CART (FINAL)
        // =========================
        public async Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null)
        {
            using var con = new NpgsqlConnection(this.DbConnection);
            await con.OpenAsync();

            foreach (var productId in productIds)
            {
                int? resolvedVariantId = null;

                // ✅ 1. If variantId दिया है → verify करो
                if (variantId.HasValue)
                {
                    var verifyCmd = new NpgsqlCommand(@"
                SELECT id FROM variant 
                WHERE id=@vid AND productid=@pid LIMIT 1", con);

                    verifyCmd.Parameters.AddWithValue("@vid", variantId.Value);
                    verifyCmd.Parameters.AddWithValue("@pid", productId);

                    var result = await verifyCmd.ExecuteScalarAsync();

                    if (result != null)
                        resolvedVariantId = variantId.Value;
                }
                else
                {
                    // ✅ 2. Product ka first variant auto pick
                    var varCmd = new NpgsqlCommand(@"
                SELECT id FROM variant 
                WHERE productid=@pid 
                ORDER BY id ASC LIMIT 1", con);

                    varCmd.Parameters.AddWithValue("@pid", productId);

                    var result = await varCmd.ExecuteScalarAsync();

                    if (result != null)
                        resolvedVariantId = Convert.ToInt32(result);
                }

                // ✅ 3. Duplicate check
                var check = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM addtocart
            WHERE (userid=@uid OR ipaddress=@ip)
            AND productid=@pid
            AND (variantid=@vid OR (variantid IS NULL AND @vid IS NULL))", con);

                check.Parameters.AddWithValue("@uid", (object?)userId ?? DBNull.Value);
                check.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
                check.Parameters.AddWithValue("@pid", productId);
                check.Parameters.Add(NullableInt("@vid", resolvedVariantId));

                var exists = (long)(await check.ExecuteScalarAsync() ?? 0);
                if (exists > 0) continue;

                // ✅ 4. FINAL PRICE LOGIC (🔥 MOST IMPORTANT)
                var insert = new NpgsqlCommand(@"
            INSERT INTO addtocart(userid, ipaddress, productid, variantid, quantity, price)
            VALUES(@uid,@ip,@pid,@vid,1,
                CASE 
                    WHEN @vid IS NOT NULL THEN 
                        COALESCE(
                            (SELECT discountprice FROM variant WHERE id=@vid),
                            (SELECT price FROM variant WHERE id=@vid)
                        )
                    ELSE 
                        (SELECT discountprice FROM product WHERE id=@pid)
                END)", con);

                insert.Parameters.AddWithValue("@uid", (object?)userId ?? DBNull.Value);
                insert.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
                insert.Parameters.AddWithValue("@pid", productId);
                insert.Parameters.Add(NullableInt("@vid", resolvedVariantId));

                await insert.ExecuteNonQueryAsync();
            }

            return "Success";
        }

        // =========================
        // ✅ ADD MULTIPLE
        // =========================
       

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        public async Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null)
        {
            using var con = new NpgsqlConnection(DbConnection);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand(@"
UPDATE addtocart
SET quantity = quantity + @change
WHERE (userid=@uid OR ipaddress=@ip)
AND productid=@pid
AND (variantid=@vid OR (variantid IS NULL AND @vid IS NULL));

DELETE FROM addtocart WHERE quantity <= 0;", con);

            cmd.Parameters.AddWithValue("@change", change);
            cmd.Parameters.AddWithValue("@uid", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pid", productId);
            cmd.Parameters.Add(NullableInt("@vid", variantId));

            await cmd.ExecuteNonQueryAsync();

            return "Success";
        }

        // =========================
        // ✅ MERGE GUEST CART
        // =========================
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            using var con = new NpgsqlConnection(DbConnection);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand(@"
UPDATE addtocart
SET userid=@uid
WHERE ipaddress=@ip;", con);

            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@ip", ipAddress);

            await cmd.ExecuteNonQueryAsync();
        }

        // =========================
        // ✅ DELETE ITEM
        // =========================
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            using var con = new NpgsqlConnection(DbConnection);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand("DELETE FROM addtocart WHERE id=@id", con);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0
                ? new OkObjectResult(new { status = true, message = "Deleted" })
                : new NotFoundObjectResult(new { status = false });
        }

        // =========================
        // ✅ CLEAR CART
        // =========================
        public async Task<IActionResult> ClearCart(int? userId, string? ipAddress)
        {
            using var con = new NpgsqlConnection(DbConnection);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand(@"
DELETE FROM addtocart
WHERE userid=@uid OR ipaddress=@ip", con);

            cmd.Parameters.AddWithValue("@uid", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            return new OkObjectResult(new { status = true });
        }
    }
}