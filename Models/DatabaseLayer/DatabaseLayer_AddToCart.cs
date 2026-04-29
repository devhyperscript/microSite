using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string ipAddress, int productId);
        Task MergeGuestCart(int userId, string ipAddress);

        Task<IActionResult> DeleteCartItem(int id);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        // ✅ userId diya → userId se fetch, warna IP se fetch
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
                        c.productid,
                        p.productname,
                        p.slug,
                        p.image,
                        p.price,
                        p.discountprice,
                        c.quantity,
                        (c.quantity * COALESCE(p.discountprice, p.price)) AS totalprice
                    FROM addtocart c
                    INNER JOIN product p ON p.id = c.productid
                    {whereClause}
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (userId.HasValue)
                        command.Parameters.AddWithValue("@UserId", userId.Value);
                    else
                        command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cartList.Add(new CartItemModel
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                userid = reader.IsDBNull(reader.GetOrdinal("userid"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("userid")),
                                productid = reader.GetInt32(reader.GetOrdinal("productid")),
                                ProductName = reader.GetString(reader.GetOrdinal("productname")),
                                Slug = reader.IsDBNull(reader.GetOrdinal("slug")) ? null : reader.GetString(reader.GetOrdinal("slug")),
                                Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
                                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                                DiscountPrice = reader.IsDBNull(reader.GetOrdinal("discountprice"))
                                    ? (decimal?)null
                                    : reader.GetDecimal(reader.GetOrdinal("discountprice")),
                                quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                                totalprice = reader.GetDecimal(reader.GetOrdinal("totalprice"))
                            });
                        }
                    }
                }
            }

            return cartList;
        }

        // AddToCart — same as before (already handles userId vs IP)
        public async Task<string> AddToCart(int? userId, string ipAddress, int productId)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                string query = userId != null ? @"
                    INSERT INTO addtocart (userid, productid, quantity, price)
                    SELECT @UserId, @ProductId, 1, p.price
                    FROM product p WHERE p.id = @ProductId
                    ON CONFLICT (userid, productid)
                    DO UPDATE SET quantity = addtocart.quantity + 1;
                " : @"
                    INSERT INTO addtocart (productid, quantity, ipaddress, price)
                    SELECT @ProductId, 1, @IpAddress, p.price
                    FROM product p WHERE p.id = @ProductId
                    ON CONFLICT (ipaddress, productid)
                    DO UPDATE SET quantity = addtocart.quantity + 1;
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return "Success";
        }

        // ✅ Login ke baad guest cart merge karo user cart mein
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(@"
                    INSERT INTO addtocart (userid, productid, quantity)
                    SELECT @UserId, productid, quantity
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


        public async Task<IActionResult> DeleteCartItem(int id)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("DELETE FROM addtocart WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                        return new OkObjectResult(new { status = true, message = "Cart item deleted" });
                    else
                        return new NotFoundObjectResult(new { status = false, message = "Cart item not found" });
                }
            }
        }
    }
}