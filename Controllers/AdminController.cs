using firstproject.Helpers;
using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly JwtHelper _jwtHelper;

        public AdminController(IBusinessLayer businessLayer, JwtHelper jwtHelper)
        {
            _businessLayer = businessLayer;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] AdminLoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Email aur Password dono required hain"
                });
            }

            var admin = await _businessLayer.GetAdminByEmail(model.Email);

            if (admin == null)
            {
                return Unauthorized(new
                {
                    status = false,
                    message = "Email ya Password galat hai"
                });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    status = false,
                    message = "Email ya Password galat hai"
                });
            }

            admin.Token = _jwtHelper.GenerateToken(admin);
            admin.PasswordHash = null;

            var clientIp = ClientIpHelper.GetClientIp(HttpContext);
            await _businessLayer.MergeGuestCartToUserAsync(clientIp, admin.Id);

            return Ok(new
            {
                status = true,
                message = "Login successful",
                data = new
                {
                    id = admin.Id,
                    token = admin.Token,
                }
            });
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _businessLayer.GetAllAdmins();

            var safeResult = result.Select(admin => new
            {
                id = admin.Id,
                firstName = admin.FirstName,
                lastName = admin.LastName,
                phone = admin.Phone,
                email = admin.Email,
                createdAt = admin.CreatedAt
            });

            return Ok(safeResult);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] AdminModel model)
        {
            var result = await _businessLayer.Add(model);

            return Ok(new
            {
                status = true,
                message = "Record successfully added",
                data = new
                {
                    id = result.Id,
                    firstName = result.FirstName,
                    lastName = result.LastName,
                    phone = result.Phone,
                    email = result.Email,
                    createdAt = result.CreatedAt
                }
            });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] AdminModel model)
        {
            var result = await _businessLayer.Edit(id, model);

            if (result == null)
                return NotFound(new { status = false, message = "Record not found" });

            return Ok(new
            {
                status = true,
                message = "Record updated successfully",
                data = new
                {
                    id = result.Id,
                    firstName = result.FirstName,
                    lastName = result.LastName,
                    phone = result.Phone,
                    email = result.Email,
                    createdAt = result.CreatedAt
                }
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _businessLayer.Delete(id);

            if (result == null)
                return NotFound(new { status = false, message = "Record not found" });

            return Ok(new
            {
                status = true,
                message = "Record deleted successfully"
            });
        }
    }
}
