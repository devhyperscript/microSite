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

        // 🔥 SAFE GUEST ID (Cookie based)
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
                Secure = false,
                IsEssential = true
            });

            return newGuestId;
        }

        // 🔥 USER ID
        private int? GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return _jwtHelper.GetUserIdFromToken(
                authHeader.Substring("Bearer ".Length).Trim()
            );
        }

        // 🔥 NORMALIZE ID (IMPORTANT FIX)
        private (int? userId, string guestId) GetIdentity()
        {
            int? userId = GetUserIdFromToken();

            if (userId.HasValue)
                return (userId, null);

            return (null, GetGuestId());
        }

        // 🔥 ADD TO CART
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            var identity = GetIdentity();

            var result = await _businessLayer.AddToCart(
                identity.userId,
                identity.guestId,
                productId
            );

            var items = await _businessLayer.GetCart(identity.userId, identity.guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Product added",
                userId = identity.userId,
                guestId = identity.guestId,
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }

        // 🔥 GET CART
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var identity = GetIdentity();

            var items = await _businessLayer.GetCart(identity.userId, identity.guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                userId = identity.userId,
                guestId = identity.guestId,
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }

        // 🔥 UPDATE QUANTITY
        [HttpPut("updatequantity")]
        public async Task<IActionResult> UpdateQuantity([FromForm] int productId, [FromForm] int change)
        {
            if (change != 1 && change != -1)
                return BadRequest(new { status = false, message = "Only +1 or -1 allowed" });

            var identity = GetIdentity();

            await _businessLayer.UpdateCartQuantity(
                identity.userId,
                identity.guestId,
                productId,
                change
            );

            var items = await _businessLayer.GetCart(identity.userId, identity.guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Cart updated",
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }

        // 🔥 DELETE ITEM
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _businessLayer.DeleteCartItem(id);
        }

        // 🔥 CLEAR CART
        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            var identity = GetIdentity();
            return await _businessLayer.ClearCart(identity.userId, identity.guestId);
        }

        // 🔥 ADD MULTIPLE
        [HttpPost("add-multiple")]
        public async Task<IActionResult> AddMultipleToCart([FromForm] List<int> productIds)
        {
            if (productIds == null || !productIds.Any())
                return BadRequest(new { status = false, message = "ProductIds required" });

            var identity = GetIdentity();

            var result = await _businessLayer.AddMultipleToCart(
                identity.userId,
                identity.guestId,
                productIds
            );

            var items = await _businessLayer.GetCart(identity.userId, identity.guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }
    }
}