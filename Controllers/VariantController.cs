using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/variant")]
    public class VariantController : Controller
    {

        private readonly IBusinessLayer _businessLayer;
        private readonly IWebHostEnvironment _env;

        public VariantController(IBusinessLayer businessLayer, IWebHostEnvironment env)
        {
            _businessLayer = businessLayer;
            _env = env;
        }

        [HttpGet("getvariant")]
        public async Task<IActionResult> GetVariant()
        {
            var variants = await _businessLayer.GetVariant();
            return Ok(variants);
        }

        [HttpPost("addvariant")]
        public async Task<IActionResult> AddVariant([FromForm] Variantmodel variant)
        {
            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // ===== Main Image =====
                if (variant.ImageFile != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(variant.ImageFile.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await variant.ImageFile.CopyToAsync(stream);
                    }

                    variant.Image = "/uploads/" + fileName;
                }

                // ===== Gallery Images =====
                List<string> gallery = new List<string>();

                if (variant.GalleryFiles != null)
                {
                    foreach (var file in variant.GalleryFiles)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        gallery.Add("/uploads/" + fileName);
                    }
                }

                variant.ImageGallery = gallery.ToArray();

                // ===== Call Business Layer =====
                var result = await _businessLayer.AddVariant(variant);

                return Ok(new { message = "Variant added", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("updatevariant/{id}")]
        public async Task<IActionResult> UpdateVariant(int id, [FromForm] Variantmodel variant)
        {


            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // ✅ Step 1: Pehle purani image paths DB se lo
            var existingVariant = await _businessLayer.GetVariantById(id);

            // ✅ Main image update
            if (variant.ImageFile != null && variant.ImageFile.Length > 0)
            {
                // ✅ Purani image delete karo
                if (!string.IsNullOrEmpty(existingVariant?.Image))
                {
                    string oldFilePath = Path.Combine(_env.WebRootPath,
                                         existingVariant.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // ✅ Nayi image save karo
                string uniqueFileName = Guid.NewGuid().ToString()
                                        + Path.GetExtension(variant.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await variant.ImageFile.CopyToAsync(stream);
                }

                variant.Image = "/uploads/" + uniqueFileName;
            }
            else
            {
                // ✅ Nai image nahi aayi toh purani hi rakhni hai
                variant.Image = existingVariant?.Image;
            }

            // ✅ Gallery images update
            if (variant.GalleryFiles != null && variant.GalleryFiles.Length > 0)
            {
                // ✅ Purani gallery images delete karo
                if (existingVariant?.ImageGallery != null)
                {
                    foreach (var oldImage in existingVariant.ImageGallery)
                    {
                        string oldFilePath = Path.Combine(_env.WebRootPath,
                                             oldImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }
                }

                // ✅ Nayi gallery images save karo
                var galleryPaths = new List<string>();

                foreach (var file in variant.GalleryFiles)
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

                variant.ImageGallery = galleryPaths.ToArray();
            }
            else
            {
                // ✅ Nayi gallery nahi aayi toh purani hi rakhni hai
                variant.ImageGallery = existingVariant?.ImageGallery;
            }

            var result = await _businessLayer.UpdateVariant(id, variant);
            return Ok(result);



        }


        [HttpDelete("deletevariant/{id}")]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            try
            {
                var existingVariant = await _businessLayer.GetVariantById(id);
                if (existingVariant == null)
                    return NotFound("Variant not found");
                // ✅ Purani main image delete karo
                if (!string.IsNullOrEmpty(existingVariant.Image))
                {
                    string oldFilePath = Path.Combine(_env.WebRootPath,
                                         existingVariant.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                // ✅ Purani gallery images delete karo
                if (existingVariant.ImageGallery != null)
                {
                    foreach (var oldImage in existingVariant.ImageGallery)
                    {
                        string oldFilePath = Path.Combine(_env.WebRootPath,
                                             oldImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }
                }
                var result = await _businessLayer.DeleteVariant(id);
                return Ok(new { message = "Variant deleted", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
    }
}
