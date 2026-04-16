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


        [HttpPost]
        [Route("addsize")]

        public async Task<IActionResult> AddSize([FromForm] Size size)
        {

            var result = await _businessLayer.AddSize(size);
            return Ok(result);
        }

        [HttpPut]
        [Route("editsize/{id}")]
        public async Task<IActionResult> EditSize(int id, [FromForm] Size size)
        {
            var result = await _businessLayer.EditSize(id, size);
            return Ok(result);

        }

        [HttpDelete]
        [Route("deletesize/{id}")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            var result = await _businessLayer.DeleteSize(id);
            return Ok(result);
        }
    }
}
