using firstproject.Models.BusinessLayer;
using firstproject.Models;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class SizeController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public SizeController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }
        [HttpGet]
        [Route("getsize")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetSize();
            return Ok(result);
        }
    }
}
