using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<Brand>> GetBrand();
        Task<IActionResult> AddBrand([FromForm] Brand brand);
        //Task<IActionResult> EditBrand(int id, [FromForm] Brand brand);
        //Task<IActionResult> DeleteBrand(int id);
    }
    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Brand>> GetBrand()
        {
            List<Brand> brands = new List<Brand>();
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "SELECT id, brandname, brandimage, isactive FROM brand WHERE isactive = true",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Brand brand = new Brand
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


        public async Task<IActionResult> AddBrand([FromForm] Brand brand)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO brand (brandname, brandimage, isactive) VALUES (@brandname, @brandimage, @isactive) RETURNING id",
                    connection))
                {
                    command.Parameters.AddWithValue("@brandname", brand.BrandName);
                    command.Parameters.AddWithValue("@brandimage", brand.BrandImage);
                    command.Parameters.AddWithValue("@isactive", brand.IsActive);
                    int newId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    brand.Id = newId;
                }
            }
            return new OkObjectResult(new { status = true, message = "Brand added successfully", data = brand });
        }




    }
}
