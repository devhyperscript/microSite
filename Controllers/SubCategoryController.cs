using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/subcategory")]
    public class SubCategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public SubCategoryController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllSubCategory();
            return Ok(result);
        }

        // 🔥 ADD
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] SubCategoryModel model)
        {
            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.SubCategoryImageUrl = imageUrl;
            }
            else
            {
                model.SubCategoryImageUrl = null;
            }

            await _businessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added"
            });
        }

        // 🔥 EDIT
        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] SubCategoryModel model)
        {
            var existing = await _businessLayer.GetSubCategoryById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "SubCategory not found"
                });

            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.SubCategoryImageUrl = imageUrl;
            }
            else
            {
                model.SubCategoryImageUrl = existing.SubCategoryImageUrl;
            }

            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }

        // 🔥 DELETE
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var existing = await _businessLayer.GetSubCategoryById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "SubCategory not found"
                });

            await _businessLayer.DeleteSubCategory(id);

            // ❗ Cloudinary delete optional (advanced)

            return Ok(new
            {
                status = true,
                message = "Record successfully deleted"
            });
        }
    }
}