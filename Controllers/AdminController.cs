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
            return  Ok(result);
        }
    }
}
