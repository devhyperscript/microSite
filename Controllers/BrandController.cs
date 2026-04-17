using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{

    [ApiController]
    [Authorize(Roles = "Admin")]
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

        [HttpPost]
        [Route("addbrand")]

        public async Task<IActionResult> Add([FromForm] Brand brand)
        {
            var result = await _businessLayer.AddBrand(brand);
            return Ok(result);
        }




    }
}
