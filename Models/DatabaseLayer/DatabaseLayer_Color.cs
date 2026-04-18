using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{

    public partial interface IDatabaseLayer
    {
        Task<List<Colormodel>> GetColor();
        Task<IActionResult> AddColor([FromForm] Colormodel color);

        Task<IActionResult> EditColor(int id, [FromForm] Colormodel color);
        Task<IActionResult> DeleteColor(int id);


    }
    public partial interface IDatabaseLayer
    {
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Colormodel>> GetColor()
        {
            List<Colormodel> colors = new List<Colormodel>();
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "SELECT id, colorname, colorcode, isactive FROM color ",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Colormodel color = new Colormodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Colorname = reader["colorname"]?.ToString(),
                                Colorcode = reader["colorcode"]?.ToString(),

                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive"))
                            };
                            colors.Add(color);
                        }
                    }
                }
            }
            return colors;
        }

        public async Task<IActionResult> AddColor([FromForm] Colormodel color)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO color (colorname, colorcode, isactive) VALUES (@colorname, @colorcode, @isactive) RETURNING id",
                    connection))
                {
                    command.Parameters.AddWithValue("@colorname", color.Colorname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@colorcode", color.Colorcode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isactive", color.Isactive);
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && int.TryParse(result.ToString(), out int newId))
                    {
                        color.Id = newId;
                        return new JsonResult(color) { StatusCode = 201 };
                    }
                    else
                    {
                        return new JsonResult(new { error = "Failed to add color" }) { StatusCode = 500 };
                    }
                }
            }

        }


        public async Task<IActionResult> EditColor(int id, [FromForm] Colormodel color)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "UPDATE color SET colorname = @colorname, colorcode = @colorcode, isactive = @isactive WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@colorname", color.Colorname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@colorcode", color.Colorcode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isactive", color.Isactive);
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult(color) { StatusCode = 200 };
                    }
                    else
                    {
                        return new JsonResult(new { error = "Failed to update color" }) { StatusCode = 500 };
                    }
                }
            }
        }
        public async Task<IActionResult> DeleteColor(int id)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "DELETE FROM color WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult(new { message = "Color deleted successfully" }) { StatusCode = 200 };
                    }
                    else
                    {
                        return new JsonResult(new { error = "Failed to delete color" }) { StatusCode = 500 };
                    }
                }
            }



        }
    }
}
