using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public CategoryController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
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

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] categoryModel model)
        {
            if (model.ImageFile == null)
                return BadRequest("ImageFile is NULL");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            model.ImageUrl = "/uploads/" + fileName;

            var result = await _businessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added"
            });
        }

        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] categoryModel model)
        {
            if (model.ImageFile != null)
            {
                // Step 1: DB se purani image ka path fetch karo
                var existing = await _businessLayer.GetCategoryById(id);

                // Step 2: Purani image file delete karo
                if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
                {
                    var oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        existing.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // Step 3: Nayi image save karo
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/uploads/" + fileName;
            }

            // Step 4: DB update karo
            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully edited"
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Step 1: Pehle image path fetch karo DB se
            var existing = await _businessLayer.GetCategoryById(id);

            // Step 2: Category DB se delete karo
            var result = await _businessLayer.DeleteCategory(id);

            if (!result)
                return NotFound(new { status = false, message = "Record not found" });

            // Step 3: Image file bhi delete karo
            if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    existing.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            return Ok(new
            {
                status = true,
                message = "Record deleted successfully"
            });
        }
    }
}