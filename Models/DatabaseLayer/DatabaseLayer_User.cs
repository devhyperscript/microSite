using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
   public partial interface IDatabaseLayer
    {
        Task<List<Usermodel>> GetUsers();


    }

    public partial interface IDatabaseLayer
    {
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {


        public async Task<List<Usermodel>> GetUsers()
        {
            List<Usermodel> users = new List<Usermodel>();
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "SELECT id, firstname, lastname, email, password,role, isactive,createdat FROM users ",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Usermodel user = new Usermodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Firstname = reader["firstname"]?.ToString(),
                                Lastname = reader["lastname"]?.ToString(),
                                Email = reader["email"]?.ToString(),
                                Password = reader["password"]?.ToString(),
                                Role = reader["role"]?.ToString(),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                Createdat = reader.GetDateTime(reader.GetOrdinal("createdat"))
                            };
                            users.Add(user);
                        }
                    }
                }   

            }
            return users;
        }




    }
}
