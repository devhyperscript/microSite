using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<IActionResult> PlaceOrder(int userId, CheckoutRequestModel request, List<CartItemModel> items, decimal grandTotal);
        Task<IActionResult> GetMyOrders(int userId);
        Task<IActionResult> GetOrderDetail(int userId, int orderId);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        // ✅ PLACE ORDER
        public async Task<IActionResult> PlaceOrder(
            int userId,
            CheckoutRequestModel request,
            List<CartItemModel> items,
            decimal grandTotal)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Step 1: orders table mein insert karo
                int orderId;
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO orders 
                        (userid, first_name, last_name, email, mobile, pincode, 
                         address, city, state, country, total_items, grand_total, payment_method)
                    VALUES 
                        (@UserId, @FirstName, @LastName, @Email, @Mobile, @Pincode,
                         @Address, @City, @State, @Country, @TotalItems, @GrandTotal, @PaymentMethod)
                    RETURNING id;
                ", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", request.LastName);
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@Mobile", request.Mobile);
                    cmd.Parameters.AddWithValue("@Pincode", request.Pincode);
                    cmd.Parameters.AddWithValue("@Address", request.Address);
                    cmd.Parameters.AddWithValue("@City", request.City);
                    cmd.Parameters.AddWithValue("@State", request.State);
                    cmd.Parameters.AddWithValue("@Country", request.Country ?? "India");
                    cmd.Parameters.AddWithValue("@TotalItems", items.Count);
                    cmd.Parameters.AddWithValue("@GrandTotal", grandTotal);
                    cmd.Parameters.AddWithValue("@PaymentMethod", request.PaymentMethod ?? "COD");

                    orderId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // Step 2: order_items mein har product insert karo
                foreach (var item in items)
                {
                    using var itemCmd = new NpgsqlCommand(@"
                        INSERT INTO order_items 
                            (order_id, product_id, product_name, image, price, discount_price, quantity, total_price)
                        VALUES 
                            (@OrderId, @ProductId, @ProductName, @Image, @Price, @DiscountPrice, @Quantity, @TotalPrice);
                    ", connection, transaction);

                    itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.productid);
                    itemCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@Image", (object?)item.Image ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Price", item.Price);
                    itemCmd.Parameters.AddWithValue("@DiscountPrice", (object?)item.DiscountPrice ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.quantity);
                    itemCmd.Parameters.AddWithValue("@TotalPrice", item.totalprice);

                    await itemCmd.ExecuteNonQueryAsync();
                }

                // Step 3: Cart clear karo
                using (var clearCmd = new NpgsqlCommand(
                    "DELETE FROM addtocart WHERE userid = @UserId", connection, transaction))
                {
                    clearCmd.Parameters.AddWithValue("@UserId", userId);
                    await clearCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    status = true,
                    message = "Order successfully place ho gaya!",
                    orderId = orderId,
                    totalItems = items.Count,
                    grandTotal = grandTotal,
                    paymentMethod = request.PaymentMethod,
                    shippingAddress = new
                    {
                        name = $"{request.FirstName} {request.LastName}",
                        email = request.Email,
                        mobile = request.Mobile,
                        address = request.Address,
                        city = request.City,
                        state = request.State,
                        pincode = request.Pincode,
                        country = request.Country
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ObjectResult(new
                {
                    status = false,
                    message = "Order place karne mein error aaya.",
                    error = ex.Message
                })
                { StatusCode = 500 };
            }
        }

        // ✅ GET MY ORDERS
        public async Task<IActionResult> GetMyOrders(int userId)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            var orders = new List<object>();

            using var cmd = new NpgsqlCommand(@"
                SELECT 
                    id, userid, first_name, last_name, email, mobile,
                    pincode, address, city, state, country,
                    total_items, grand_total, payment_method, order_status, created_at
                FROM orders
                WHERE userid = @UserId
                ORDER BY created_at DESC;
            ", connection);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orders.Add(new
                {
                    id = reader.GetInt32("id"),
                    firstName = reader.GetString("first_name"),
                    lastName = reader.GetString("last_name"),
                    email = reader.GetString("email"),
                    mobile = reader.GetString("mobile"),
                    pincode = reader.GetString("pincode"),
                    address = reader.GetString("address"),
                    city = reader.GetString("city"),
                    state = reader.GetString("state"),
                    country = reader.GetString("country"),
                    totalItems = reader.GetInt32("total_items"),
                    grandTotal = reader.GetDecimal("grand_total"),
                    paymentMethod = reader.GetString("payment_method"),
                    orderStatus = reader.GetString("order_status"),
                    createdAt = reader.GetDateTime("created_at")
                });
            }

            return new OkObjectResult(new
            {
                status = true,
                totalOrders = orders.Count,
                data = orders
            });
        }

        // ✅ GET ORDER DETAIL
        public async Task<IActionResult> GetOrderDetail(int userId, int orderId)
        {
            using var connection = new NpgsqlConnection(this.DbConnection);
            await connection.OpenAsync();

            // Order check karo
            object? order = null;
            using (var cmd = new NpgsqlCommand(@"
                SELECT 
                    id, userid, first_name, last_name, email, mobile,
                    pincode, address, city, state, country,
                    total_items, grand_total, payment_method, order_status, created_at
                FROM orders
                WHERE id = @OrderId AND userid = @UserId;
            ", connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    order = new
                    {
                        id = reader.GetInt32("id"),
                        firstName = reader.GetString("first_name"),
                        lastName = reader.GetString("last_name"),
                        email = reader.GetString("email"),
                        mobile = reader.GetString("mobile"),
                        pincode = reader.GetString("pincode"),
                        address = reader.GetString("address"),
                        city = reader.GetString("city"),
                        state = reader.GetString("state"),
                        country = reader.GetString("country"),
                        totalItems = reader.GetInt32("total_items"),
                        grandTotal = reader.GetDecimal("grand_total"),
                        paymentMethod = reader.GetString("payment_method"),
                        orderStatus = reader.GetString("order_status"),
                        createdAt = reader.GetDateTime("created_at")
                    };
                }
            }

            if (order == null)
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Order nahi mila."
                });

            // Order items lo
            var items = new List<object>();
            using (var itemCmd = new NpgsqlCommand(@"
                SELECT 
                    id, product_id, product_name, image,
                    price, discount_price, quantity, total_price, created_at
                FROM order_items
                WHERE order_id = @OrderId
                ORDER BY id ASC;
            ", connection))
            {
                itemCmd.Parameters.AddWithValue("@OrderId", orderId);

                using var itemReader = await itemCmd.ExecuteReaderAsync();
                while (await itemReader.ReadAsync())
                {
                    items.Add(new
                    {
                        id = itemReader.GetInt32("id"),
                        productId = itemReader.GetInt32("product_id"),
                        productName = itemReader.GetString("product_name"),
                        image = itemReader.IsDBNull("image") ? null : itemReader.GetString("image"),
                        price = itemReader.GetDecimal("price"),
                        discountPrice = itemReader.IsDBNull("discount_price") ? (decimal?)null : itemReader.GetDecimal("discount_price"),
                        quantity = itemReader.GetInt32("quantity"),
                        totalPrice = itemReader.GetDecimal("total_price")
                    });
                }
            }

            return new OkObjectResult(new
            {
                status = true,
                order = order,
                orderItems = items
            });
        }
    }
}