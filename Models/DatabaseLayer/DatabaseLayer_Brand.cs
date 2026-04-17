using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {

        Task<List<Brandmodel>> GetBrand();
        //Task<List<Brand>> GetBrands();
        Task<Brandmodel> Add(Brandmodel model);
    }
//descriotion ki jge img  and wwwroot folder ke under uplaod ka ka usme 
public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Brandmodel>> GetBrand()
        {
            List<Brandmodel> brands = new List<Brandmodel   >();

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

    }


}
