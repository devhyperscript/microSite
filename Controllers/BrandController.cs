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

        [HttpPost("addbrand")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] Brandmodel model)
        {
            // Image optional
            if (model.ImageFile != null)
            {
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
            }

            // agar image nahi aayi to BrandImage null hi rahega
            var result = await _businessLayer.Add(model);
            return Ok(result);
        }

        [HttpPut("editbrand/{id}")]
        [Authorize]

        //public async Task<IActionResult> Edit(int id, [FromForm] Brandmodel model)
        //{
        //    // Image logic BusinessLayer mein handle hogi
        //    var result = await _businessLayer.Edit(id, model);
        //    return Ok(result);
        //}


        public async Task<IActionResult> Edit(int id, [FromForm] Brandmodel model)
        {
            // Step 1: Pehle existing record fetch karo
            var existingBrand = await _businessLayer.GetBrandById(id);
            if (existingBrand == null)
                return NotFound(new
                {
                    status = false,
                    message = "Brand not found"
                });

            if (model.ImageFile != null)
            {
                // Step 2: Purani image delete karo
                if (!string.IsNullOrEmpty(existingBrand.BrandImage))
                {
                    var oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        existingBrand.BrandImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
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

                model.BrandImage = "/uploads/" + fileName;
            }
            else
            {
                // Agar nai image nahi aayi toh purani rakho
                model.BrandImage = existingBrand.BrandImage;
            }

            // Step 4: DB update karo
            var result = await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }



        [HttpDelete("deletebrand/{id}")]
        [Authorize]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var result = await _businessLayer.DeleteBrand(id);
        //    return Ok(new
        //    {
        //        status = true,
        //        message = "Brand deleted successfully"
        //    });
        //}


        public async Task<IActionResult> DeleteBrand(int id)
        {
            // Step 1: Pehle existing record fetch karo
            var existingBrand = await _businessLayer.GetBrandById(id);
            if (existingBrand == null)
                return NotFound(new
                {
                    status = false,
                    message = "Brand not found"
                });
            // Step 2: Brand DB se delete karo
            var result = await _businessLayer.DeleteBrand(id);
            if (!(result is OkResult))
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });
            // Step 3: Agar image exist karti hai toh delete karo
            if (!string.IsNullOrEmpty(existingBrand.BrandImage))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    existingBrand.BrandImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
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