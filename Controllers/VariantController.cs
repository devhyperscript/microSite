using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/variant")]
    public class VariantController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public VariantController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("getvariant")]
        public async Task<IActionResult> GetVariant()
        {
            var variants = await _businessLayer.GetVariant();
            return Ok(variants);
        }

        // 🔥 ADD VARIANT
        [HttpPost("addvariant")]
        [Authorize]
        public async Task<IActionResult> AddVariant([FromForm] Variantmodel variant)
        {
            try
            {
                // ✅ Main Image
                if (variant.ImageFile != null)
                {
                    var imageUrl = await _cloudinary.UploadImageAsync(variant.ImageFile);
                    variant.Image = imageUrl;
                }

                // ✅ Gallery Images
                if (variant.GalleryFiles != null && variant.GalleryFiles.Length > 0)
                {
                    var galleryList = new List<string>();

                    foreach (var file in variant.GalleryFiles)
                    {
                        if (file != null)
                        {
                            var imageUrl = await _cloudinary.UploadImageAsync(file);
                            galleryList.Add(imageUrl);
                        }
                    }

                    variant.ImageGallery = galleryList.ToArray();
                }

                var result = await _businessLayer.AddVariant(variant);

                return Ok(new { message = "Variant added", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔥 UPDATE VARIANT
        [HttpPut("updatevariant/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateVariant(int id, [FromForm] Variantmodel variant)
        {
            var existing = await _businessLayer.GetVariantById(id);

            if (existing == null)
                return NotFound(new { message = "Variant not found" });

            // ✅ Main Image
            if (variant.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(variant.ImageFile);
                variant.Image = imageUrl;
            }
            else
            {
                variant.Image = existing.Image;
            }

            // ✅ Gallery Images
            if (variant.GalleryFiles != null && variant.GalleryFiles.Length > 0)
            {
                var galleryList = new List<string>();

                foreach (var file in variant.GalleryFiles)
                {
                    if (file != null)
                    {
                        var imageUrl = await _cloudinary.UploadImageAsync(file);
                        galleryList.Add(imageUrl);
                    }
                }

                variant.ImageGallery = galleryList.ToArray();
            }
            else
            {
                variant.ImageGallery = existing.ImageGallery;
            }

            var result = await _businessLayer.UpdateVariant(id, variant);
            return Ok(result);
        }

        // 🔥 DELETE VARIANT
        [HttpDelete("deletevariant/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var existing = await _businessLayer.GetVariantById(id);

            if (existing == null)
                return NotFound(new { message = "Variant not found" });

            await _businessLayer.DeleteVariant(id);

            // ❗ Cloudinary delete optional (advanced)

            return Ok(new { message = "Variant deleted" });
        }
    }
}