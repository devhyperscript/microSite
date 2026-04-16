using Microsoft.AspNetCore.Mvc;

using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
   public  partial interface IDatabaseLayer
    {
        Task<List<Size>> GetSize();
    }
    public partial interface IDatabaseLayer
    {

    }
    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Size>> GetSize()
        {
            List<Size> sizes = new List<Size>();

            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(
                    "SELECT id, size_name, description, is_active FROM sizes WHERE is_active = true",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Size size = new Size
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                SizeName = reader["size_name"]?.ToString(),
                                Description = reader["description"]?.ToString(),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
                            };

                            sizes.Add(size);
                        }
                    }
                }
            }

            return sizes;
        }
    }
}
