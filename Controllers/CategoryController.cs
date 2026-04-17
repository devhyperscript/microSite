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
            {
                return BadRequest("ImageFile is NULL ❌");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

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
                message = "Record successfully added",
                data = result
            });
        }
    }

}