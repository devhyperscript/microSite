using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/subcategory")]
    public class SubCategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public SubCategoryController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        [HttpGet("get")]

        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllSubCategory();
            return Ok(result);
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] SubCategoryModel model)
        {
            // ✅ Agar image hai tabhi upload karo
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.SubCategoryImageUrl = "/uploads/" + fileName;
            }
            else
            {
                // ✅ Agar image nahi hai to null ya empty set karo
                model.SubCategoryImageUrl = null;
            }

            var result = await _businessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added",

            });
        }

        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] SubCategoryModel model)
        {
            // Step 1: Pehle existing record fetch karo
            var existingSubCategory = await _businessLayer.GetSubCategoryById(id);
            if (existingSubCategory == null)
                return NotFound(new
                {
                    status = false,
                    message = "SubCategory not found"
                });

            if (model.ImageFile != null)
            {
                // Step 2: Purani image delete karo
                if (!string.IsNullOrEmpty(existingSubCategory.SubCategoryImageUrl))
                {
                    var oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        existingSubCategory.SubCategoryImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
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

                model.SubCategoryImageUrl = "/uploads/" + fileName;
            }
            else
            {
                // Agar nai image nahi aayi toh purani rakho
                model.SubCategoryImageUrl = existingSubCategory.SubCategoryImageUrl;
            }

            // Step 4: DB update karo
            var result = await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            // Step 1: Pehle existing record fetch karo
            var existingSubCategory = await _businessLayer.GetSubCategoryById(id);
            if (existingSubCategory == null)
                return NotFound(new
                {
                    status = false,
                    message = "SubCategory not found"
                });
            // Step 2: SubCategory DB se delete karo
            var result = await _businessLayer.DeleteSubCategory(id);
            if (!result)
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });
            // Step 3: Agar image exist karti hai toh delete karo
            if (!string.IsNullOrEmpty(existingSubCategory.SubCategoryImageUrl))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    existingSubCategory.SubCategoryImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }
            return Ok(new
            {
                status = true,
                message = "Record successfully deleted"
            });
        }
    }
}
