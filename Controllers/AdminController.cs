using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBusinessLayer _binessLayer;

        public AdminController(IBusinessLayer businessLayer)
        {
            _binessLayer = businessLayer;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _binessLayer.GetAllAdmins();
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] AdminModel model)
        {
            var result = await _binessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added",
                data = result
            });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] AdminModel model)
        {
            var result = await _binessLayer.Edit(id, model);

            if (result == null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });
            }

            return Ok(new
            {
                status = true,
                message = "Record updated successfully",
                data = result
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _binessLayer.Delete(id);

            if (result==null)
            {
                return NotFound(new
                {
                    status = false,
                    message = "Record not found"
                });
            }
            return Ok(new
            {
                status = true,
                message = "Record deleted successfully"
            });
        }
    }

    }



