using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class BrandController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public BrandController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        [Route("getbrand")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetBrand();
            return Ok(result);
        }

        // 🔥 ADD BRAND (Cloudinary Upload)
        [HttpPost("addbrand")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] Brandmodel model)
        {
            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.BrandImage = imageUrl;
            }

            var result = await _businessLayer.Add(model);
            return Ok(result);
        }

        // 🔥 EDIT BRAND
        [HttpPut("editbrand/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] Brandmodel model)
        {
            var existingBrand = await _businessLayer.GetBrandById(id);

            if (existingBrand == null)
                return NotFound(new
                {
                    status = false,
                    message = "Brand not found"
                });

            if (model.ImageFile != null)
            {
                // 👉 New image upload (old delete optional)
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.BrandImage = imageUrl;
            }
            else
            {

            }
            {
                model.BrandImage = existingBrand.BrandImage;
            }

            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }

        // 🔥 DELETE BRAND
        [HttpDelete("deletebrand/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var existingBrand = await _businessLayer.GetBrandById(id);

            if (existingBrand == null)
                return NotFound(new
                {
                    status = false,
                    message = "Brand not found"
                });

            await _businessLayer.DeleteBrand(id);

            // ❗ Cloudinary delete optional (advanced)
            return Ok(new
            {
                status = true,
                message = "Record successfully deleted"
            });
        }
    }
}