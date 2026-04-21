using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/childCategory")]
    public class ChildCategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public ChildCategoryController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllChildCategory();
            return Ok(result);
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] childCategoryModel model)
        {
            if (model.ImageFile == null)
                return BadRequest(new
                {
                    status = false,
                    message = "ImageFile is NULL",
                    contentType = Request.ContentType  // ← Postman mein dekho kya aa raha hai
                });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }
            model.ChildCategoryImageUrl = "/uploads/" + fileName;
            var result = await _businessLayer.Add(model);
            return Ok(new
            {
                status = true,
                message = "Child Category added successfully",
                //data = result
            });

        }

        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] childCategoryModel model)
        {
            // 🔥 Step 1: Get existing record
            var existingData = await _businessLayer.GetChildCategoryById(id);
            if (existingData == null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Child Category not found"
                });
            }

            // 🔥 Step 2: If new image uploaded → delete old image
            if (model.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(existingData.ChildCategoryImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingData.ChildCategoryImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath); // ❌ OLD IMAGE DELETE
                    }
                }

                // 🔥 Step 3: Upload new image
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.ChildCategoryImageUrl = "/uploads/" + fileName;
            }
            else
            {
                // 🔥 Agar new image nahi aayi → old image hi use karo
                model.ChildCategoryImageUrl = existingData.ChildCategoryImageUrl;
            }

            var result = await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Child Category updated successfully"
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]

        public async Task<IActionResult> Delete(int id)
        {
            // 🔥 Step 1: Get existing data
            var existingData = await _businessLayer.GetChildCategoryById(id);
            if (existingData == null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Child Category not found"
                });
            }

            // 🔥 Step 2: Delete image from folder
            if (!string.IsNullOrEmpty(existingData.ChildCategoryImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingData.ChildCategoryImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // ❌ IMAGE DELETE
                }
            }

            // 🔥 Step 3: Delete DB record
            var result = await _businessLayer.DeleteChildCategory(id);

            return Ok(new
            {
                status = true,
                message = "Child Category deleted successfully"
            });
        }
    }
}