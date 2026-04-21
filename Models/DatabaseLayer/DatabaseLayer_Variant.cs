using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;

namespace firstproject.Models.DatabaseLayer
{
   public partial interface IDatabaseLayer
    {
        Task<List<Variantmodel>> GetVariant();
        Task<IActionResult> AddVariant([FromForm] Variantmodel variant);
    }

    public partial class DatabaseLayer : IDatabaseLayer
    {


        public async Task<List<Variantmodel>> GetVariant()
        {
            using (var connection = new NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand(@"
        SELECT 
            v.id,
            v.variantname,
            v.productid,
            v.sizeid,
            v.colorid,
            v.price,
            v.stock,
            v.sku,
            v.image,
            v.imagegallery,
            v.isactive,
            v.createdat,

            ARRAY_AGG(DISTINCT s.size_name) AS size_name,
            ARRAY_AGG(DISTINCT c.colorname) AS colorname

        FROM variant v
        LEFT JOIN sizes s ON s.id = ANY(v.sizeid)
        LEFT JOIN color c ON c.id = ANY(v.colorid)
        GROUP BY v.id;
    ", connection);

                var reader = await command.ExecuteReaderAsync();
                var variants = new List<Variantmodel>();

                while (await reader.ReadAsync())
                {
                    variants.Add(new Variantmodel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        VariantName = reader.IsDBNull(reader.GetOrdinal("variantname"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("variantname")),

                        ProductId = reader.GetInt32(reader.GetOrdinal("productid")),

                        SizeId = reader.IsDBNull(reader.GetOrdinal("sizeid"))
                            ? null
                            : reader.GetFieldValue<int[]>(reader.GetOrdinal("sizeid")),

                        ColorId = reader.IsDBNull(reader.GetOrdinal("colorid"))
                            ? null
                            : reader.GetFieldValue<int[]>(reader.GetOrdinal("colorid")),

                        SizeNames = reader.IsDBNull(reader.GetOrdinal("size_name"))
                            ? null
                            : reader.GetFieldValue<string[]>(reader.GetOrdinal("size_name")),

                        ColorNames = reader.IsDBNull(reader.GetOrdinal("colorname"))
                            ? null
                            : reader.GetFieldValue<string[]>(reader.GetOrdinal("colorname")),

                        Price = reader.GetDecimal(reader.GetOrdinal("price")),
                        Stock = reader.GetInt32(reader.GetOrdinal("stock")),

                        Sku = reader.IsDBNull(reader.GetOrdinal("sku"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("sku")),

                        Image = reader.IsDBNull(reader.GetOrdinal("image"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("image")),

                        ImageGallery = reader.IsDBNull(reader.GetOrdinal("imagegallery"))
                            ? null
                            : reader.GetFieldValue<string[]>(reader.GetOrdinal("imagegallery")),

                        IsActive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat"))
                    });
                }

                return variants;
            }
        }



        public async Task<IActionResult> AddVariant([FromForm] Variantmodel variant)
        {
            using (var connection = new NpgsqlConnection(DbConnection))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand("INSERT INTO variant (productid, variantname, sizeid, colorid, price, stock, sku, image,imagegallery ,isactive ) VALUES (@productid, @variantname, @sizeid, @colorid, @price, @stock, @sku, @image,@imagegallery, @isactive )RETURNING id, createdat", connection);
                command.Parameters.AddWithValue("@productid", variant.ProductId);
                command.Parameters.AddWithValue("@variantname", (object)variant.VariantName ?? DBNull.Value);
                command.Parameters.Add(new NpgsqlParameter("@sizeid",
     NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer)
                {
                    Value = variant.SizeId ?? Array.Empty<int>()
                });

                command.Parameters.Add(new NpgsqlParameter("@colorid",
                    NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer)
                {
                    Value = variant.ColorId ?? Array.Empty<int>()
                });

                command.Parameters.AddWithValue("@price", variant.Price);
                command.Parameters.AddWithValue("@stock", variant.Stock);
                command.Parameters.AddWithValue("@sku", (object)variant.Sku ?? DBNull.Value);
                command.Parameters.AddWithValue("@image", (object)variant.Image ?? DBNull.Value);
              
                command.Parameters.Add(new NpgsqlParameter("@imagegallery", NpgsqlDbType.Array | NpgsqlDbType.Text)
                {
                    Value = (object?)variant.ImageGallery ?? DBNull.Value
                });
                command.Parameters.AddWithValue("@isactive", variant.IsActive);
                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                    return new JsonResult(new { status = true, message = "Variant added successfully" });
                else
                    return new JsonResult(new { status = false, message = "Failed to add variant" });
            }


        }
    }

}
