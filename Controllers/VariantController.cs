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
    }
}
