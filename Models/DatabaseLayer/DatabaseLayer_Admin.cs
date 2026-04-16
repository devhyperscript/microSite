namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<AdminModel>> GetAllAdmins();
    }

    public partial interface IDatabaseLayer
    {

    }

    public partial class DataBaseLayer : IDatabaseLayer
    {
        public async Task<List<AdminModel>> GetAllAdmins()
        {
            var admins = new List<AdminModel>();

            using (var connection = new Npgsql.NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();

                var command = new Npgsql.NpgsqlCommand(
                    @"SELECT ""Id"", ""FirstName"", ""LastName"", ""Phone"", ""Email"", ""PasswordHash"", ""CreatedAt"" 
              FROM ""admin"" 
              ORDER BY ""Id"" DESC", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        admins.Add(new AdminModel
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Email = reader.GetString(4),
                            PasswordHash = reader.GetString(5),
                            CreatedAt = reader.GetDateTime(6)
                        });
                    }
                }
            }

            return admins;
        }
    }
}
