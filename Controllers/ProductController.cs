using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {


        private readonly IBusinessLayer _businessLayer;
        private readonly IWebHostEnvironment _env; // ✅ add karo

        public ProductController(IBusinessLayer businessLayer, IWebHostEnvironment env)
        {
            _businessLayer = businessLayer;
            _env = env; // ✅ add karo
        }

        [HttpGet("getproduct")]
        public async Task<IActionResult> GetProduct()
        {
            var products = await _businessLayer.GetProduct();
            return Ok(products);
        }
        [HttpPost("addproduct")]
        [Authorize]
        public async Task<IActionResult> AddProduct([FromForm] Productmodel product)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder); // ✅ folder na ho toh bana do

            // ✅ Main image save
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString()
                                        + Path.GetExtension(product.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.Image = "/uploads/" + uniqueFileName; // ✅ DB mein yahi path jayegi
            }

            // ✅ Gallery images save
            if (product.GalleryFiles != null && product.GalleryFiles.Length > 0)
            {
                var galleryPaths = new List<string>();

                foreach (var file in product.GalleryFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString()
                                                + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        galleryPaths.Add("/uploads/" + uniqueFileName);
                    }
                }

                product.ImageGallery = galleryPaths.ToArray(); // ✅ DB mein yahi paths jayengi
            }

            var result = await _businessLayer.AddProduct(product);
            return Ok(result);
        }


        [HttpPut("updateproduct/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Productmodel product)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // ✅ Step 1: Pehle purani image paths DB se lo
            var existingProduct = await _businessLayer.GetProductById(id);

            // ✅ Main image update
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                // ✅ Purani image delete karo
                if (!string.IsNullOrEmpty(existingProduct?.Image))
                {
                    string oldFilePath = Path.Combine(_env.WebRootPath,
                                         existingProduct.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // ✅ Nayi image save karo
                string uniqueFileName = Guid.NewGuid().ToString()
                                        + Path.GetExtension(product.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.Image = "/uploads/" + uniqueFileName;
            }
            else
            {
                // ✅ Nai image nahi aayi toh purani hi rakhni hai
                product.Image = existingProduct?.Image;
            }

            // ✅ Gallery images update
            if (product.GalleryFiles != null && product.GalleryFiles.Length > 0)
            {
                // ✅ Purani gallery images delete karo
                if (existingProduct?.ImageGallery != null)
                {
                    foreach (var oldImage in existingProduct.ImageGallery)
                    {
                        string oldFilePath = Path.Combine(_env.WebRootPath,
                                             oldImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }
                }

                // ✅ Nayi gallery images save karo
                var galleryPaths = new List<string>();

                foreach (var file in product.GalleryFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString()
                                                + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        galleryPaths.Add("/uploads/" + uniqueFileName);
                    }
                }

                product.ImageGallery = galleryPaths.ToArray();
            }
            else
            {
                // ✅ Nayi gallery nahi aayi toh purani hi rakhni hai
                product.ImageGallery = existingProduct?.ImageGallery;
            }

            var result = await _businessLayer.UpdateProduct(id, product);
            return Ok(result);
        }



        [HttpDelete("deleteproduct/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // ✅ Step 1: Pehle DB se product lo (image paths ke liye)
            var existingProduct = await _businessLayer.GetProductById(id);

            if (existingProduct == null)
                return NotFound(new { status = false, message = "Product not found" });

            // ✅ Step 2: Main image delete karo folder se
            if (!string.IsNullOrEmpty(existingProduct.Image))
            {
                string oldFilePath = Path.Combine(_env.WebRootPath,
                                     existingProduct.Image.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            // ✅ Step 3: Gallery images delete karo folder se
            if (existingProduct.ImageGallery != null && existingProduct.ImageGallery.Length > 0)
            {
                foreach (var image in existingProduct.ImageGallery)
                {
                    string oldFilePath = Path.Combine(_env.WebRootPath,
                                         image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
            }

            // ✅ Step 4: DB se delete karo
            var result = await _businessLayer.DeleteProduct(id);
            return Ok(result);
        }

    }
}
