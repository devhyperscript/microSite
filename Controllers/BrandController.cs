using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{


    [ApiController]
    [Route("api/admin")]
    public class BrandController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        public BrandController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }
        [HttpGet]
        [Route("getbrand")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetBrand();
            return Ok(result);
        }

        //[HttpPost]
        //[Route("addbrand")]

        [HttpPost("addbrand")]
   
        public async Task<IActionResult> Add([FromForm] Brandmodel model)
        {
            if (model.ImageFile == null)
                return BadRequest("Image required");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            model.BrandImage = "/uploads/" + fileName;

            var result = await _businessLayer.Add(model);

            return Ok(result);
        }


    }
}



