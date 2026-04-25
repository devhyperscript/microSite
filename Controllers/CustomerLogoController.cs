using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]



    [Route("api/admin")]

    public class CustomerLogoController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public CustomerLogoController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        [HttpGet]
        [Route("getcustomerlogo")]

        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetCustomerLogo();
            return Ok(result);
        }

        [HttpPost("addcustomerlogo")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] customermodel model)
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

                model.customerimage = "/uploads/" + fileName;
            }

            // agar image nahi aayi to customerimage null hi rahega
            var result = await _businessLayer.Add(model);
            return Ok(result);
        }

        [HttpPut("editcustomerlogo/{id}")]
        [Authorize]

        public async Task<IActionResult> Edit(int id, [FromForm] customermodel model)
        {
            // Step 1: Pehle existing record fetch karo
            var existingBrand = await _businessLayer.GetCustomerLogoById(id);
            if (existingBrand == null)
                return NotFound(new
                {
                    status = false,
                    message = "Customer logo not found"
                });

            if (model.ImageFile != null)
            {
                // Step 2: Purani image delete karo
                if (!string.IsNullOrEmpty(existingBrand.customerimage))
                {
                    var oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        existingBrand.customerimage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
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

                model.customerimage = "/uploads/" + fileName;
            }
            else
            {
                // Agar nai image nahi aayi toh purani rakho
                model.customerimage = existingBrand.customerimage;
            }

            // Step 4: DB update karo
            var result = await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }



        [HttpDelete("deletecustomerlogo/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCustomerLogo(int id)
        {
            // Step 1: Get existing record
            var existingCustomerLogo = await _businessLayer.GetCustomerLogoById(id);
            if (existingCustomerLogo == null)
                return NotFound(new
                {
                    status = false,
                    message = "Customer logo not found"
                });

            // Step 2: Delete from DB
            var result = await _businessLayer.DeleteCustomerLogo(id);

            // ✅ FIX: correct check
            if (result == null)
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });

            // Step 3: Delete image file
            if (!string.IsNullOrEmpty(existingCustomerLogo.customerimage))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    existingCustomerLogo.customerimage.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar)
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