using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/childCategory")]
    public class ChildCategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public ChildCategoryController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllChildCategory();
            return Ok(result);
        }

        // 🔥 ADD
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] childCategoryModel model)
        {
            try
            {
                if (model.ImageFile != null)
                {
                    var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                    model.ChildCategoryImageUrl = imageUrl;
                }
                else
                {
                    model.ChildCategoryImageUrl = null;
                }

                var result = await _businessLayer.Add(model);

                return Ok(new
                {
                    status = true,
                    message = "Child Category added successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // 🔥 EDIT
        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] childCategoryModel model)
        {
            var existingData = await _businessLayer.GetChildCategoryById(id);

            if (existingData == null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Child Category not found"
                });
            }

            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.ChildCategoryImageUrl = imageUrl;
            }
            else
            {
                model.ChildCategoryImageUrl = existingData.ChildCategoryImageUrl;
            }

            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Child Category updated successfully"
            });
        }

        // 🔥 DELETE
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var existingData = await _businessLayer.GetChildCategoryById(id);

            if (existingData == null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Child Category not found"
                });
            }

            await _businessLayer.DeleteChildCategory(id);

            // ❗ Cloudinary delete (optional advanced)

            return Ok(new
            {
                status = true,
                message = "Child Category deleted successfully"
            });
        }
    }
}