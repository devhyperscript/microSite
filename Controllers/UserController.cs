using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {

        private readonly IBusinessLayer _businessLayer;

        public UserController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }
         [HttpGet("getusers")]
         public async Task<IActionResult> GetUsers()
         {
                var result = await _businessLayer.GetUsers();
                return Ok(result);
         }

    }
}
