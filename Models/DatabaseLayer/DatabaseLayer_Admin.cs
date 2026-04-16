using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<AdminModel>> GetAllAdmins();
        Task<IActionResult> Add([FromForm] AdminModel model);

        Task<IActionResult> Edit([FromForm] AdminModel model);
    }

    public partial interface IDatabaseLayer
    {

    }

    public partial class DatabaseLayer : IDatabaseLayer
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


        public async Task<IActionResult> Add([FromForm] AdminModel model)
        {
            using (var connection = new Npgsql.NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();
                var command = new Npgsql.NpgsqlCommand(
                    @"INSERT INTO ""admin"" (""FirstName"", ""LastName"", ""Phone"", ""Email"", ""PasswordHash"", ""CreatedAt"") 
              VALUES (@FirstName, @LastName, @Phone, @Email, @PasswordHash, @CreatedAt) 
              RETURNING ""Id""", connection);
                command.Parameters.AddWithValue("@FirstName", model.FirstName);
                command.Parameters.AddWithValue("@LastName", model.LastName);
                command.Parameters.AddWithValue("@Phone", (object)model.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email);
                command.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                var newId = (int)await command.ExecuteScalarAsync();
                model.Id = newId;
            }
            return new OkObjectResult(model);
        }


        public async Task<IActionResult> Edit([FromForm] AdminModel model)
        {
            using (var connection = new Npgsql.NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();
                var command = new Npgsql.NpgsqlCommand(
                    @"UPDATE ""admin"" 
              SET ""FirstName"" = @FirstName, 
                  ""LastName"" = @LastName, 
                  ""Phone"" = @Phone, 
                  ""Email"" = @Email, 
                  ""PasswordHash"" = @PasswordHash
              WHERE ""Id"" = @Id", connection);
                command.Parameters.AddWithValue("@Id", model.Id);
                command.Parameters.AddWithValue("@FirstName", model.FirstName);
                command.Parameters.AddWithValue("@LastName", model.LastName);
                command.Parameters.AddWithValue("@Phone", (object)model.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email);
                command.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return new NotFoundResult();
                }
            }
            return new OkObjectResult(model);


        
    }
}
