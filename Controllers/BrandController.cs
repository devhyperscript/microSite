using firstproject.Models.BusinessLayer;
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




    }
}
