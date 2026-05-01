using firstproject.Models;
using firstproject.Models.BusinessLayer;
using firstproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class CustomerLogoController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly CloudinaryService _cloudinary;

        public CustomerLogoController(IBusinessLayer businessLayer, CloudinaryService cloudinary)
        {
            _businessLayer = businessLayer;
            _cloudinary = cloudinary;
        }

        [HttpGet("getcustomerlogo")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetCustomerLogo();
            return Ok(result);
        }

        // 🔥 ADD
        [HttpPost("addcustomerlogo")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] customermodel model)
        {
            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.customerimage = imageUrl;
            }

            var result = await _businessLayer.Add(model);
            return Ok(result);
        }

        // 🔥 EDIT
        [HttpPut("editcustomerlogo/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] customermodel model)
        {
            var existing = await _businessLayer.GetCustomerLogoById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "Customer logo not found"
                });

            if (model.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
                model.customerimage = imageUrl;
            }
            else
            {
                model.customerimage = existing.customerimage;
            }

            await _businessLayer.Edit(id, model);

            return Ok(new
            {
                status = true,
                message = "Record successfully updated"
            });
        }

        // 🔥 DELETE
        [HttpDelete("deletecustomerlogo/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCustomerLogo(int id)
        {
            var existing = await _businessLayer.GetCustomerLogoById(id);

            if (existing == null)
                return NotFound(new
                {
                    status = false,
                    message = "Customer logo not found"
                });

            await _businessLayer.DeleteCustomerLogo(id);

            // ❗ Cloudinary delete optional (advanced)

            return Ok(new
            {
                status = true,
                message = "Record successfully deleted"
            });
        }
    }
}