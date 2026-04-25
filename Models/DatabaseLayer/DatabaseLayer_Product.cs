using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Npgsql;
using NpgsqlTypes;

namespace firstproject.Models.DatabaseLayer
{
    public partial interface IDatabaseLayer
    {
        Task<List<Productmodel>> GetProduct();
        Task<IActionResult> AddProduct(Productmodel product);
        Task<Productmodel?> GetProductById(int id);
        Task<IActionResult> UpdateProduct(int id, Productmodel product);
        Task<IActionResult> DeleteProduct(int id);


    }
    public partial interface IDatabaseLayer
    {
    }


    public partial class DatabaseLayer : IDatabaseLayer
    {
        public async Task<List<Productmodel>> GetProduct()
        {
            List<Productmodel> products = new List<Productmodel>();
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(@"
            SELECT 
                p.id,
                p.productname,
                p.slug,
p.sku,
p.shortdescription,
                p.description,
                p.price,
                p.stock,
                p.categoryid,
                p.subcategoryid,
                p.childcategoryid,
                p.brandid,
                p.sizeids,
                p.colorids,
                p.image,
                p.imagegallery,
                p.isactive,
                p.createdat,

                -- ✅ Names
                c.""Name""          AS categoryname,
                sc.""SubCategoryName"" AS subcategoryname,
                cc.""ChildCategoryName"" AS childcategoryname,
                b.brandname         AS brandname,

                -- ✅ Size names array
                ARRAY(
                    SELECT s.size_name 
                    FROM sizes s 
                    WHERE s.id = ANY(p.sizeids)
                ) AS sizenames,

                -- ✅ Color names array
                ARRAY(
                    SELECT col.colorname 
                    FROM color col 
                    WHERE col.id = ANY(p.colorids)
                ) AS colornames

            FROM product p
            LEFT JOIN category c         ON p.categoryid = c.""Id""
            LEFT JOIN subcategory sc      ON p.subcategoryid = sc.""Id""
            LEFT JOIN childcategory cc    ON p.childcategoryid = cc.""Id""
            LEFT JOIN brand b             ON p.brandid = b.id
        ", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Productmodel product = new Productmodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                ProductName = reader["productname"]?.ToString(),
                                Slug = reader.IsDBNull(reader.GetOrdinal("slug"))
                                                ? null : reader["slug"].ToString(),
                                Sku = reader.IsDBNull(reader.GetOrdinal("sku"))
                                                ? null : reader["sku"].ToString(),
                                ShortDescription = reader.IsDBNull(reader.GetOrdinal("shortdescription"))
                                                ? null : reader["shortdescription"].ToString(),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                                ? null : reader["description"].ToString(),
                                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                                Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("categoryid")),
                                SubCategoryId = reader.GetInt32(reader.GetOrdinal("subcategoryid")),
                                ChildCategoryId = reader.IsDBNull(reader.GetOrdinal("childcategoryid"))
                                                ? null : reader.GetInt32(reader.GetOrdinal("childcategoryid")),
                                BrandId = reader.IsDBNull(reader.GetOrdinal("brandid"))
                                                ? null : reader.GetInt32(reader.GetOrdinal("brandid")),
                                SizeIds = reader.IsDBNull(reader.GetOrdinal("sizeids"))
                                                ? null : (int[])reader["sizeids"],
                                ColorIds = reader.IsDBNull(reader.GetOrdinal("colorids"))
                                                ? null : (int[])reader["colorids"],
                                Image = reader.IsDBNull(reader.GetOrdinal("image"))
                                                ? null : reader["image"].ToString(),
                                ImageGallery = reader.IsDBNull(reader.GetOrdinal("imagegallery"))
                                                ? null : (string[])reader["imagegallery"],
                                IsActive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat")),

                                // ✅ Names
                                CategoryName = reader.IsDBNull(reader.GetOrdinal("categoryname"))
                                                ? null : reader["categoryname"].ToString(),
                                SubCategoryName = reader.IsDBNull(reader.GetOrdinal("subcategoryname"))
                                                ? null : reader["subcategoryname"].ToString(),
                                ChildCategoryName = reader.IsDBNull(reader.GetOrdinal("childcategoryname"))
                                                ? null : reader["childcategoryname"].ToString(),
                                BrandName = reader.IsDBNull(reader.GetOrdinal("brandname"))
                                                ? null : reader["brandname"].ToString(),

                                // ✅ Size names
                                SizeNames = reader.IsDBNull(reader.GetOrdinal("sizenames"))
                                                ? null
                                                : ((string[])reader["sizenames"]).ToList(),

                                // ✅ Color names
                                ColorNames = reader.IsDBNull(reader.GetOrdinal("colornames"))
                                                ? null
                                                : ((string[])reader["colornames"]).ToList(),
                            };

                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }




        public async Task<IActionResult> AddProduct(Productmodel product)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                // 🔥 OPTIONAL: slug duplicate check
                var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM product WHERE slug=@slug", connection);
                checkCmd.Parameters.AddWithValue("@slug", product.Slug);
                var count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    product.Slug = product.Slug + "-" + Guid.NewGuid().ToString().Substring(0, 5);
                }

                using (var command = new NpgsqlCommand(@"
            INSERT INTO product 
            (productname, slug, sku, shortdescription, description, price, stock, categoryid, subcategoryid, childcategoryid, brandid, sizeids, colorids, image, imagegallery, isactive, createdat) 
            VALUES 
            (@productname,@slug, @sku, @shortdescription, @description, @price, @stock, @categoryid, @subcategoryid, @childcategoryid, @brandid, @sizeids, @colorids, @image, @imagegallery, @isactive, @createdat)", connection))
                {
                    command.Parameters.AddWithValue("@productname", product.ProductName);
                    command.Parameters.AddWithValue("@slug", product.Slug); // ✅ FIXED
                    command.Parameters.AddWithValue("@sku", (object)product.Sku ?? DBNull.Value);
                    command.Parameters.AddWithValue("@shortdescription", (object)product.ShortDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@stock", product.Stock);
                    command.Parameters.AddWithValue("@categoryid", product.CategoryId);
                    command.Parameters.AddWithValue("@subcategoryid", product.SubCategoryId);
                    command.Parameters.AddWithValue("@childcategoryid", (object?)product.ChildCategoryId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@brandid", (object?)product.BrandId ?? DBNull.Value);

                    command.Parameters.Add(new NpgsqlParameter("@sizeids", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                    {
                        Value = (object?)product.SizeIds ?? DBNull.Value
                    });

                    command.Parameters.Add(new NpgsqlParameter("@colorids", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                    {
                        Value = (object?)product.ColorIds ?? DBNull.Value
                    });

                    command.Parameters.AddWithValue("@image", (object?)product.Image ?? DBNull.Value);

                    command.Parameters.Add(new NpgsqlParameter("@imagegallery", NpgsqlDbType.Array | NpgsqlDbType.Text)
                    {
                        Value = (object?)product.ImageGallery ?? DBNull.Value
                    });

                    command.Parameters.AddWithValue("@isactive", product.IsActive);
                    command.Parameters.AddWithValue("@createdat", DateTime.UtcNow);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        return new OkObjectResult(new { status = true, message = "Product added successfully" });
                    else
                        return new BadRequestObjectResult(new { status = false, message = "Failed to add product" });
                }
            }
        }



        public async Task<IActionResult> UpdateProduct(int id, Productmodel product)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(@"
            UPDATE product SET 
            productname = @productname, 
slug = @slug,
sku = @sku,
shortdescription = @shortdescription,
            description = @description, 
            price = @price, 
            stock = @stock, 
            categoryid = @categoryid,
            subcategoryid = @subcategoryid,
            childcategoryid = @childcategoryid,
            brandid = @brandid,
            sizeids = @sizeids,
            colorids = @colorids,
            image = @image,
            imagegallery = @imagegallery,
            isactive = @isactive
            WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@productname", product.ProductName);
                    command.Parameters.AddWithValue("@slug", product.Slug ?? Guid.NewGuid().ToString()); // Generate slug if not provided
                    command.Parameters.AddWithValue("@sku", (object?)product.Sku ?? DBNull.Value);
                    command.Parameters.AddWithValue("@shortdescription", (object?)product.ShortDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@description", (object?)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@stock", product.Stock);
                    command.Parameters.AddWithValue("@categoryid", product.CategoryId);        // ✅ fix
                    command.Parameters.AddWithValue("@subcategoryid", product.SubCategoryId);
                    command.Parameters.AddWithValue("@childcategoryid", (object?)product.ChildCategoryId ?? DBNull.Value); // ✅ fix
                    command.Parameters.AddWithValue("@brandid", (object?)product.BrandId ?? DBNull.Value);

                    command.Parameters.Add(new NpgsqlParameter("@sizeids", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                    {
                        Value = (object?)product.SizeIds ?? DBNull.Value
                    });

                    command.Parameters.Add(new NpgsqlParameter("@colorids", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                    {
                        Value = (object?)product.ColorIds ?? DBNull.Value
                    });

                    command.Parameters.AddWithValue("@image", (object?)product.Image ?? DBNull.Value);

                    command.Parameters.Add(new NpgsqlParameter("@imagegallery", NpgsqlDbType.Array | NpgsqlDbType.Text)
                    {
                        Value = (object?)product.ImageGallery ?? DBNull.Value
                    });

                    command.Parameters.AddWithValue("@isactive", product.IsActive);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        return new OkObjectResult(new { status = true, message = "Product updated successfully" });
                    else
                        return new NotFoundObjectResult(new { status = false, message = "Product not found" });
                }
            }
        }


        // Implementation
        public async Task<Productmodel?> GetProductById(int id)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(@"
            SELECT id, productname, slug, sku, shortdescription, description, price, stock,
                   categoryid, subcategoryid, childcategoryid,
                   brandid, sizeids, colorids,
                   image, imagegallery, isactive, createdat
            FROM product WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Productmodel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                ProductName = reader["productname"]?.ToString(),
                                Slug = reader.IsDBNull(reader.GetOrdinal("slug"))
                                                ? null : reader["slug"].ToString(),
                                Sku = reader.IsDBNull(reader.GetOrdinal("sku")) 
                                                ? null : reader["sku"].ToString(),
                                ShortDescription = reader.IsDBNull(reader.GetOrdinal("shortdescription"))
                                                ? null : reader["shortdescription"].ToString(),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                                ? null : reader["description"].ToString(),
                                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                                Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("categoryid")),
                                SubCategoryId = reader.GetInt32(reader.GetOrdinal("subcategoryid")),
                                ChildCategoryId = reader.IsDBNull(reader.GetOrdinal("childcategoryid"))
                                                ? null : reader.GetInt32(reader.GetOrdinal("childcategoryid")),
                                BrandId = reader.IsDBNull(reader.GetOrdinal("brandid"))
                                                ? null : reader.GetInt32(reader.GetOrdinal("brandid")),
                                SizeIds = reader.IsDBNull(reader.GetOrdinal("sizeids"))
                                                ? null : (int[])reader["sizeids"],
                                ColorIds = reader.IsDBNull(reader.GetOrdinal("colorids"))
                                                ? null : (int[])reader["colorids"],
                                Image = reader.IsDBNull(reader.GetOrdinal("image"))
                                                ? null : reader["image"].ToString(),
                                ImageGallery = reader.IsDBNull(reader.GetOrdinal("imagegallery"))
                                                ? null : (string[])reader["imagegallery"],
                                IsActive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat"))
                            };
                        }
                    }
                }
            }
            return null;
        }



        public async Task<IActionResult> DeleteProduct(int id)
        {
            using (var connection = new NpgsqlConnection(this.DbConnection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("DELETE FROM product WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                        return new OkObjectResult(new { status = true, message = "Product deleted successfully" });
                    else
                        return new NotFoundObjectResult(new { status = false, message = "Product not found" });
                }
            }


        }
    }
}