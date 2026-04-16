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
            try
            {
                var result = await _binessLayer.Add(model);

                if (result == null)
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Failed to add record"
                    });
                }

                return Ok(new
                {
                    status = true,
                    message = "Record successfully added",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "Something went wrong",
                    error = ex.Message
                });
            }
        }
    }
}
