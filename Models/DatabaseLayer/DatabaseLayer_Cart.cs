using Npgsql;
using firstproject.Models;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task AddOrUpdateCartItemAsync(int? userId, string? clientIp, int productId, int quantity);
        Task<List<CartItem>> GetCartAsync(int? userId, string? clientIp);
        Task MergeGuestCartToUserAsync(string clientIp, int userId);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        private static readonly SemaphoreSlim CartSchemaGate = new(1, 1);
        private static bool _cartSchemaEnsured;

        private async Task EnsureCartSchemaAsync()
        {
            if (_cartSchemaEnsured) return;
            await CartSchemaGate.WaitAsync();
            try
            {
                if (_cartSchemaEnsured) return;

                await using var connection = new NpgsqlConnection(DbConnection);
                await connection.OpenAsync();
                const string sql = @"
CREATE TABLE IF NOT EXISTS ""cart_items"" (
  ""Id"" SERIAL PRIMARY KEY,
  ""ProductId"" INT NOT NULL,
  ""Quantity"" INT NOT NULL DEFAULT 1,
  ""ClientIp"" VARCHAR(64),
  ""UserId"" INT,
  ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  ""UpdatedAt"" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS ""ix_cart_items_guest_ip"" ON ""cart_items"" (""ClientIp"") WHERE ""UserId"" IS NULL;
CREATE INDEX IF NOT EXISTS ""ix_cart_items_user_id"" ON ""cart_items"" (""UserId"") WHERE ""UserId"" IS NOT NULL;
";
                await using var command = new NpgsqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
                _cartSchemaEnsured = true;
            }
            finally
            {
                CartSchemaGate.Release();
            }
        }

        public async Task AddOrUpdateCartItemAsync(int? userId, string? clientIp, int productId, int quantity)
        {
            await EnsureCartSchemaAsync();
            var utc = DateTime.UtcNow;

            await using var connection = new NpgsqlConnection(DbConnection);
            await connection.OpenAsync();

            int? existingId = null;
            int existingQty = 0;

            if (userId.HasValue)
            {
                await using (var find = new NpgsqlCommand(
                    @"SELECT ""Id"", ""Quantity"" FROM ""cart_items"" 
WHERE ""ProductId""=@ProductId AND ""UserId""=@UserId", connection))
                {
                    find.Parameters.AddWithValue("@ProductId", productId);
                    find.Parameters.AddWithValue("@UserId", userId.Value);
                    await using var reader = await find.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        existingId = reader.GetInt32(0);
                        existingQty = reader.GetInt32(1);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(clientIp))
                    return;

                await using (var find = new NpgsqlCommand(
                    @"SELECT ""Id"", ""Quantity"" FROM ""cart_items"" 
WHERE ""ProductId""=@ProductId AND ""UserId"" IS NULL AND ""ClientIp""=@ClientIp", connection))
                {
                    find.Parameters.AddWithValue("@ProductId", productId);
                    find.Parameters.AddWithValue("@ClientIp", clientIp);
                    await using var reader = await find.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        existingId = reader.GetInt32(0);
                        existingQty = reader.GetInt32(1);
                    }
                }
            }

            if (existingId.HasValue)
            {
                var newQty = existingQty + quantity;
                if (newQty <= 0)
                {
                    await using var del = new NpgsqlCommand(@"DELETE FROM ""cart_items"" WHERE ""Id""=@Id", connection);
                    del.Parameters.AddWithValue("@Id", existingId.Value);
                    await del.ExecuteNonQueryAsync();
                    return;
                }

                await using var upd = new NpgsqlCommand(
                    @"UPDATE ""cart_items"" SET ""Quantity""=@Quantity, ""UpdatedAt""=@UpdatedAt WHERE ""Id""=@Id", connection);
                upd.Parameters.AddWithValue("@Quantity", newQty);
                upd.Parameters.AddWithValue("@UpdatedAt", utc);
                upd.Parameters.AddWithValue("@Id", existingId.Value);
                await upd.ExecuteNonQueryAsync();
            }
            else
            {
                if (quantity <= 0) return;

                if (userId.HasValue)
                {
                    await using var ins = new NpgsqlCommand(
                        @"INSERT INTO ""cart_items"" (""ProductId"",""Quantity"",""ClientIp"",""UserId"",""CreatedAt"",""UpdatedAt"")
VALUES (@ProductId,@Quantity,NULL,@UserId,@CreatedAt,@UpdatedAt)", connection);
                    ins.Parameters.AddWithValue("@ProductId", productId);
                    ins.Parameters.AddWithValue("@Quantity", quantity);
                    ins.Parameters.AddWithValue("@UserId", userId.Value);
                    ins.Parameters.AddWithValue("@CreatedAt", utc);
                    ins.Parameters.AddWithValue("@UpdatedAt", utc);
                    await ins.ExecuteNonQueryAsync();
                }
                else if (!string.IsNullOrEmpty(clientIp))
                {
                    await using var ins = new NpgsqlCommand(
                        @"INSERT INTO ""cart_items"" (""ProductId"",""Quantity"",""ClientIp"",""UserId"",""CreatedAt"",""UpdatedAt"")
VALUES (@ProductId,@Quantity,@ClientIp,NULL,@CreatedAt,@UpdatedAt)", connection);
                    ins.Parameters.AddWithValue("@ProductId", productId);
                    ins.Parameters.AddWithValue("@Quantity", quantity);
                    ins.Parameters.AddWithValue("@ClientIp", clientIp);
                    ins.Parameters.AddWithValue("@CreatedAt", utc);
                    ins.Parameters.AddWithValue("@UpdatedAt", utc);
                    await ins.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<CartItem>> GetCartAsync(int? userId, string? clientIp)
        {
            await EnsureCartSchemaAsync();
            var list = new List<CartItem>();

            await using var connection = new NpgsqlConnection(DbConnection);
            await connection.OpenAsync();

            NpgsqlCommand cmd;
            if (userId.HasValue)
            {
                cmd = new NpgsqlCommand(
                    @"SELECT ""Id"",""ProductId"",""Quantity"",""ClientIp"",""UserId"",""CreatedAt"",""UpdatedAt""
FROM ""cart_items"" WHERE ""UserId""=@UserId ORDER BY ""Id""", connection);
                cmd.Parameters.AddWithValue("@UserId", userId.Value);
            }
            else
            {
                if (string.IsNullOrEmpty(clientIp)) return list;
                cmd = new NpgsqlCommand(
                    @"SELECT ""Id"",""ProductId"",""Quantity"",""ClientIp"",""UserId"",""CreatedAt"",""UpdatedAt""
FROM ""cart_items"" WHERE ""UserId"" IS NULL AND ""ClientIp""=@ClientIp ORDER BY ""Id""", connection);
                cmd.Parameters.AddWithValue("@ClientIp", clientIp);
            }

            await using (cmd)
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new CartItem
                    {
                        Id = reader.GetInt32(0),
                        ProductId = reader.GetInt32(1),
                        Quantity = reader.GetInt32(2),
                        ClientIp = reader.IsDBNull(3) ? null : reader.GetString(3),
                        UserId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        CreatedAt = reader.GetDateTime(5),
                        UpdatedAt = reader.GetDateTime(6)
                    });
                }
            }

            return list;
        }

        public async Task MergeGuestCartToUserAsync(string clientIp, int userId)
        {
            if (string.IsNullOrEmpty(clientIp)) return;

            await EnsureCartSchemaAsync();
            var utc = DateTime.UtcNow;

            await using var connection = new NpgsqlConnection(DbConnection);
            await connection.OpenAsync();
            await using var tx = await connection.BeginTransactionAsync();

            try
            {
                await using (var upd = new NpgsqlCommand(
                    @"UPDATE ""cart_items"" SET ""UserId""=@UserId, ""ClientIp""=NULL, ""UpdatedAt""=@UpdatedAt
WHERE ""ClientIp""=@ClientIp AND ""UserId"" IS NULL", connection, tx))
                {
                    upd.Parameters.AddWithValue("@UserId", userId);
                    upd.Parameters.AddWithValue("@ClientIp", clientIp);
                    upd.Parameters.AddWithValue("@UpdatedAt", utc);
                    await upd.ExecuteNonQueryAsync();
                }

                await ConsolidateDuplicateProductsForUserAsync(connection, tx, userId);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static async Task ConsolidateDuplicateProductsForUserAsync(
            NpgsqlConnection connection, NpgsqlTransaction tx, int userId)
        {
            List<(int productId, List<(int id, int qty)>)> duplicateGroups = new();

            await using (var q = new NpgsqlCommand(
                @"SELECT ""ProductId"", ""Id"", ""Quantity"" FROM ""cart_items"" WHERE ""UserId""=@UserId ORDER BY ""Id""",
                connection, tx))
            {
                q.Parameters.AddWithValue("@UserId", userId);
                await using var reader = await q.ExecuteReaderAsync();
                var byProduct = new Dictionary<int, List<(int id, int qty)>>();
                while (await reader.ReadAsync())
                {
                    var pid = reader.GetInt32(0);
                    var id = reader.GetInt32(1);
                    var qty = reader.GetInt32(2);
                    if (!byProduct.TryGetValue(pid, out var rows))
                    {
                        rows = new List<(int id, int qty)>();
                        byProduct[pid] = rows;
                    }
                    rows.Add((id, qty));
                }

                foreach (var kv in byProduct)
                {
                    if (kv.Value.Count > 1)
                        duplicateGroups.Add((kv.Key, kv.Value));
                }
            }

            var utc = DateTime.UtcNow;
            foreach (var (_, rows) in duplicateGroups)
            {
                var keepId = rows[0].id;
                var totalQty = rows.Sum(r => r.qty);
                await using (var u = new NpgsqlCommand(
                    @"UPDATE ""cart_items"" SET ""Quantity""=@Quantity, ""UpdatedAt""=@UpdatedAt WHERE ""Id""=@Id",
                    connection, tx))
                {
                    u.Parameters.AddWithValue("@Quantity", totalQty);
                    u.Parameters.AddWithValue("@UpdatedAt", utc);
                    u.Parameters.AddWithValue("@Id", keepId);
                    await u.ExecuteNonQueryAsync();
                }

                for (var i = 1; i < rows.Count; i++)
                {
                    await using var d = new NpgsqlCommand(@"DELETE FROM ""cart_items"" WHERE ""Id""=@Id", connection, tx);
                    d.Parameters.AddWithValue("@Id", rows[i].id);
                    await d.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
