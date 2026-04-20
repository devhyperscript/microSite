using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactController : ControllerBase
    {


        private readonly IBusinessLayer _businessLayer;

        public ContactController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }
        [HttpGet("getcontact")]
        public async Task<IActionResult> GetContact()
        {
            var result = await _businessLayer.GetContact();
            var response = new
            {
                status = true,
                message = "Contact fetched successfully",
                data = result.Select(c => new
                {
                    id = c.Id,
                    name = c.FullName,
                    email = c.Email,
                    subject = c.Subject,
                    message = c.Message,
                    phone = c.PhoneNumber,

                    createdat = c.CreatedAt
                })
            };
            return Ok(response);
        }



        [HttpPost("addcontact")]
        public async Task<IActionResult> AddContact([FromForm] Contactmodel model)
        {
            var result = await _businessLayer.AddContact(model);
            return Ok(result);
        }
    }
}
