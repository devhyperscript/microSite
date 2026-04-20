using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/subcategory")]
    public class SubCategoryController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public SubCategoryController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        [HttpGet("get")]
        
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllSubCategory();
            return Ok(result);
        }
    }
}
