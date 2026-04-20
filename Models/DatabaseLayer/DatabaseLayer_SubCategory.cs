using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<SubCategoryModel>> GetAllSubCategory();
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<SubCategoryModel>> GetAllSubCategory()
        {
            var subCategories = new List<SubCategoryModel>();

            using var connection = new NpgsqlConnection(DbConnection);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(@"
    SELECT 
        ""Id"",
        ""CategoryId"",
        ""SubCategoryName"",
        ""SubCategoryImageUrl"",
        ""Status"",
        ""CreatedAt""
    FROM subcategory
    ORDER BY ""Id"" DESC
", connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var subCategory = new SubCategoryModel()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),

                        // ✅ correct column name (same as DB)
                        SubCategoryName = reader.GetString(reader.GetOrdinal("subCategoryName")),

                        // ✅ NULL safe
                        SubCategoryImageUrl = reader.IsDBNull(reader.GetOrdinal("subCategoryImageUrl"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("subCategoryImageUrl")),

                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),

                        Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                    };

                    subCategories.Add(subCategory);
                }
            }

            return subCategories;
        }
    }
}
