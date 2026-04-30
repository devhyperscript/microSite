using firstproject.Helpers;
using firstproject.Models.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly JwtHelper _jwtHelper;

        public CartController(IBusinessLayer businessLayer, JwtHelper jwtHelper)
        {
            _businessLayer = businessLayer;
            _jwtHelper = jwtHelper;
        }

        private string GetGuestId()
        {
            if (Request.Cookies.TryGetValue("guest_id", out var existingId)
                && !string.IsNullOrEmpty(existingId))
            {
                return existingId;
            }

            var newGuestId = "guest_" + Guid.NewGuid().ToString("N");

            Response.Cookies.Append("guest_id", newGuestId, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });

            return newGuestId;
        }

        private int? GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return _jwtHelper.GetUserIdFromToken(
                authHeader.Substring("Bearer ".Length).Trim()
            );
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            int? userId = GetUserIdFromToken();
            string guestId = userId.HasValue ? "" : GetGuestId();

            var result = await _businessLayer.AddToCart(userId, guestId, productId);

            if (result == "AlreadyInCart")
                return Ok(new { status = false, message = "Product already cart mein hai" });

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Product cart mein add ho gaya",
                userId = userId,
                guestId = userId == null ? guestId : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            int? userId = GetUserIdFromToken();
            string guestId = userId.HasValue ? "" : GetGuestId();

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                userId = userId,
                guestId = userId == null ? guestId : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        [HttpPut("updatequantity")]
        public async Task<IActionResult> UpdateQuantity([FromForm] int productId, [FromForm] int change)
        {
            if (change != 1 && change != -1)
                return BadRequest(new { status = false, message = "change sirf +1 ya -1 hona chahiye" });

            int? userId = GetUserIdFromToken();
            string guestId = userId.HasValue ? "" : GetGuestId();

            await _businessLayer.UpdateCartQuantity(userId, guestId, productId, change);

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = change == 1 ? "Quantity badh gayi (+1)" : "Quantity kam ho gayi (-1)",
                userId = userId,
                guestId = userId == null ? guestId : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _businessLayer.DeleteCartItem(id);
        }

        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            int? userId = GetUserIdFromToken();
            string guestId = userId.HasValue ? "" : GetGuestId();

            return await _businessLayer.ClearCart(userId, guestId);
        }
    }
}