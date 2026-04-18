using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {

        Task<List<Brandmodel>> GetBrand();
        //Task<List<Brand>> GetBrands();
        Task<Brandmodel> Add(Brandmodel model);
        Task<IActionResult> Edit(int id, [FromForm] Brandmodel model);
        Task<IActionResult> DeleteBrand(int id);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Brandmodel>> GetBrand()
        {
            List<Brandmodel> brands = new List<Brandmodel>();


            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(
                    "SELECT id, brandname, brandimage, isactive FROM brand ",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Brandmodel brand = new Brandmodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                BrandName = reader["brandname"]?.ToString(),
                                BrandImage = reader["brandimage"]?.ToString(),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("isactive"))
                            };

                            brands.Add(brand);
                        }
                    }
                }
            }

            return brands;
        }


        public async Task<Brandmodel> Add(Brandmodel model)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO brand (brandname, brandimage, isactive) VALUES (@brandname, @brandimage, @isactive) RETURNING id",
                    connection))
                {
                    command.Parameters.AddWithValue("@brandname", model.BrandName);
                    command.Parameters.AddWithValue("@brandimage", model.BrandImage);
                    command.Parameters.AddWithValue("@isactive", true);
                    var id = (int)await command.ExecuteScalarAsync();
                    model.Id = id;
                }
            }
            return model;
        }

        public async Task<IActionResult> Edit(int id, [FromForm] Brandmodel model)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "UPDATE brand SET brandname = @brandname, brandimage = @brandimage, isactive = @isactive WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@brandname", model.BrandName);
                    command.Parameters.AddWithValue("@brandimage", model.BrandImage);
                    command.Parameters.AddWithValue("@isactive", model.IsActive);
                    command.Parameters.AddWithValue("@id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                        return new OkObjectResult(model);
                    else
                        return new NotFoundResult();
                }
            }



        }



        public async Task<IActionResult> DeleteBrand(int id)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "DELETE FROM brand WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                        return new OkResult();
                    else
                        return new NotFoundResult();
                }
            }
        }
    }


}
