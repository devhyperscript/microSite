using firstproject.Models;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<categoryModel>> GetAllCategory();
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<categoryModel>> GetAllCategory()
        {
            var categories = new List<categoryModel>();

            using (var connection = new NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    @"SELECT ""Id"", ""Name"", ""Status"", ""ImageUrl"", ""CreatedAt""
                      FROM ""category""
                      ORDER BY ""Id"" DESC",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var category = new categoryModel
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("ImageUrl")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                        };

                        categories.Add(category);
                    }
                }
            }

            return categories;
        }
    }
}