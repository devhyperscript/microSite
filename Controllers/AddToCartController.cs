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

        // ===================== USER OR GUEST =====================
        private int? GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return _jwtHelper.GetUserIdFromToken(authHeader.Replace("Bearer ", "").Trim());
        }

        private string GetGuestId()
        {
            var guestId = Request.Cookies["guest_id"];

            if (!string.IsNullOrEmpty(guestId))
                return guestId;

            guestId = "guest_" + Guid.NewGuid().ToString("N");

            Response.Cookies.Append("guest_id", guestId, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                SameSite = SameSiteMode.Lax,
                Secure = false,
                IsEssential = true
            });

            return guestId;
        }

        private (int? userId, string guestId) GetIdentity()
        {
            var userId = GetUserIdFromToken();
            var guestId = userId.HasValue ? null : GetGuestId();

            return (userId, guestId);
        }

        // ===================== ADD TO CART =====================
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            var (userId, guestId) = GetIdentity();

            var result = await _businessLayer.AddToCart(userId, guestId, productId);

            if (result == "AlreadyInCart")
                return Ok(new { status = false, message = "Product already in cart" });

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Product cart mein add ho gaya",
                userId,
                guestId,
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }

        // ===================== GET CART =====================
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var (userId, guestId) = GetIdentity();

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                userId,
                guestId,
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }

        // ===================== UPDATE QTY =====================
        [HttpPut("updatequantity")]
        public async Task<IActionResult> UpdateQuantity([FromForm] int productId, [FromForm] int change)
        {
            if (change != 1 && change != -1)
                return BadRequest(new { status = false, message = "Invalid change value" });

            var (userId, guestId) = GetIdentity();

            await _businessLayer.UpdateCartQuantity(userId, guestId, productId, change);

            var items = await _businessLayer.GetCart(userId, guestId);
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

        // ===================== DELETE ITEM =====================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _businessLayer.DeleteCartItem(id);
        }

        // ===================== CLEAR CART =====================
        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            var (userId, guestId) = GetIdentity();

            return await _businessLayer.ClearCart(userId, guestId);
        }

        // ===================== MULTIPLE ADD =====================
        [HttpPost("add-multiple")]
        public async Task<IActionResult> AddMultipleToCart([FromForm] List<int> productIds)
        {
            if (productIds == null || !productIds.Any())
                return BadRequest(new { status = false, message = "ProductIds required" });

            var (userId, guestId) = GetIdentity();

            var result = await _businessLayer.AddMultipleToCart(userId, guestId, productIds);

            var items = await _businessLayer.GetCart(userId, guestId);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Multiple products added",
                totalItems = items.Count,
                grandTotal,
                data = items
            });
        }
    }
}