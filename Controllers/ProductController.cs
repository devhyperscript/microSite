using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public ProductController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("getproduct")]
        public async Task<IActionResult> GetProduct()
        {
            var products = await _businessLayer.GetProduct();
            return Ok(products);
        }

        // 🔥 ADD PRODUCT
        [HttpPost("addproduct")]
        [Authorize]
        public async Task<IActionResult> AddProduct([FromForm] Productmodel product)
        {
            // ✅ Main image upload
            if (product.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(product.ImageFile);
                product.Image = imageUrl;
            }

            // ✅ Gallery images upload
            if (product.GalleryFiles != null && product.GalleryFiles.Length > 0)
            {
                var galleryList = new List<string>();

                foreach (var file in product.GalleryFiles)
                {
                    if (file != null)
                    {
                        var imageUrl = await _cloudinary.UploadImageAsync(file);
                        galleryList.Add(imageUrl);
                    }
                }

                product.ImageGallery = galleryList.ToArray();
            }

            // ✅ Slug
            product.Slug = !string.IsNullOrEmpty(product.Slug)
                ? product.Slug
                : GenerateSlug(product.ProductName);

            var result = await _businessLayer.AddProduct(product);
            return Ok(result);
        }

        // 🔥 UPDATE PRODUCT
        [HttpPut("updateproduct/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Productmodel product)
        {
            var existingProduct = await _businessLayer.GetProductById(id);

            if (existingProduct == null)
                return NotFound(new { status = false, message = "Product not found" });

            // ✅ Main image update
            if (product.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(product.ImageFile);
                product.Image = imageUrl;
            }
            else
            {
                product.Image = existingProduct.Image;
            }

            // ✅ Gallery update
            if (product.GalleryFiles != null && product.GalleryFiles.Length > 0)
            {
                var galleryList = new List<string>();

                foreach (var file in product.GalleryFiles)
                {
                    if (file != null)
                    {
                        var imageUrl = await _cloudinary.UploadImageAsync(file);
                        galleryList.Add(imageUrl);
                    }
                }

                product.ImageGallery = galleryList.ToArray();
            }
            else
            {
                product.ImageGallery = existingProduct.ImageGallery;
            }

            var result = await _businessLayer.UpdateProduct(id, product);
            return Ok(result);
        }

        // 🔥 DELETE PRODUCT
        [HttpDelete("deleteproduct/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _businessLayer.GetProductById(id);

            if (existingProduct == null)
                return NotFound(new { status = false, message = "Product not found" });

            await _businessLayer.DeleteProduct(id);

            // ❗ Cloudinary delete optional (advanced)

            return Ok(new
            {
                status = true,
                message = "Product deleted successfully"
            });
        }

        [HttpGet("getproductbyid/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _businessLayer.GetProductById(id);

            if (product == null)
                return NotFound(new { status = false, message = "Product not found" });

            return Ok(new { status = true, data = product });
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Guid.NewGuid().ToString();

            text = text.ToLower();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            text = text.Replace(" ", "-");

            return text;
        }
    }
}