using firstproject.Helpers;
using firstproject.Models;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [AllowAnonymous]
    public class CartController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;

        public CartController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        private async Task<int?> TryGetUserIdFromTokenAsync()
        {
            var auth = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (!auth.Succeeded || auth.Principal == null) return null;
            var idClaim = auth.Principal.FindFirst(ClaimTypes.NameIdentifier)
                ?? auth.Principal.FindFirst("id");
            if (idClaim == null || !int.TryParse(idClaim.Value, out var id)) return null;
            return id;
        }

        /// <summary>
        /// Guest: items stored by <see cref="ClientIpHelper.GetClientIp"/>. Logged-in: send Authorization Bearer; items stored by user id.
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int product_id, [FromForm] int quantity = 1)
        {
            if (product_id <= 0 || quantity == 0)
            {
                return BadRequest(new { status = false, message = "Invalid product_id or quantity" });
            }

            var userId = await TryGetUserIdFromTokenAsync();
            var clientIp = ClientIpHelper.GetClientIp(HttpContext);

            if (!userId.HasValue && string.IsNullOrEmpty(clientIp))
            {
                return BadRequest(new { status = false, message = "Could not determine client IP" });
            }

            await _businessLayer.AddOrUpdateCartItemAsync(userId, userId.HasValue ? null : clientIp, product_id, quantity);

            return Ok(new
            {
                status = true,
                message = "Added to cart",
                data = new
                {
                    product_id,
                    quantity,
                    user_id = userId,
                    client_ip = userId.HasValue ? (string?)null : clientIp
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = await TryGetUserIdFromTokenAsync();
            var clientIp = ClientIpHelper.GetClientIp(HttpContext);

            if (!userId.HasValue && string.IsNullOrEmpty(clientIp))
            {
                return Ok(new { status = true, data = Array.Empty<CartItem>() });
            }

            var items = await _businessLayer.GetCartAsync(userId, userId.HasValue ? null : clientIp);
            return Ok(new { status = true, data = items });
        }
    }
}
