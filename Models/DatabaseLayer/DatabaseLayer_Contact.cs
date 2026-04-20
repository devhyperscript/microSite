using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{


    public partial interface IDatabaseLayer
    {
        Task<List<Contactmodel>> GetContact();
        Task<IActionResult> AddContact([FromForm] Contactmodel model);
    }

    public partial interface IDatabaseLayer
    {
    }


    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Contactmodel>> GetContact()
        {
            List<Contactmodel> contacts = new List<Contactmodel>();
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "SELECT id, fullname, email, subject, message, phone, createdat FROM contact ",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Contactmodel contact = new Contactmodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FullName = reader["fullname"]?.ToString(),
                                Email = reader["email"]?.ToString(),
                                Subject = reader["subject"]?.ToString(),
                                Message = reader["message"]?.ToString(),
                                PhoneNumber = reader["phone"]?.ToString(),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat"))
                            };
                            contacts.Add(contact);
                        }
                    }
                }
            }
            return contacts;
        }
    }
}
    
