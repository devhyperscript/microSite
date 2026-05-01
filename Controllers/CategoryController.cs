using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public CategoryController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("get")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllCategory();

            var safeResult = result.Select(category => new
            {
                category.id,
                category.Name,
                category.ImageUrl,
                category.Status,
                category.CreatedAt
            });

            return Ok(safeResult);
        }

        // 🔥 ADD CATEGORY (Cloudinary Upload)
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] categoryModel model)
        {
            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.ImageUrl = imageUrl;
            }

            await _businessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added"
            });
        }

        // 🔥 EDIT CATEGORY
        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] categoryModel model)
        {
            var existing = await _businessLayer.GetCategoryById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });

            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.ImageUrl = imageUrl;
            }
            else
            {
                model.ImageUrl = existing.ImageUrl;
            }

            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully edited"
            });
        }

        // 🔥 DELETE CATEGORY
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existing = await _businessLayer.GetCategoryById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });

            await _businessLayer.DeleteCategory(id);

            // ❗ Cloudinary delete (optional advanced)

            return Ok(new
            {
                status = true,
                message = "Record deleted successfully"
            });
        }
    }
}