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

        // ===================== USER =====================
        private int? GetUserIdFromToken()
        {
            var auth = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
                return null;

            return _jwtHelper.GetUserIdFromToken(auth.Replace("Bearer ", "").Trim());
        }

        // ===================== GUEST COOKIE =====================
        private string GetGuestId()
        {
            var guestId = Request.Cookies["guest_id"];

            if (!string.IsNullOrEmpty(guestId))
                return guestId;

            guestId = "guest_" + Guid.NewGuid().ToString("N");

            Response.Cookies.Append("guest_id", guestId, new CookieOptions
            {
                HttpOnly = false,          // 🔥 frontend access allowed
                SameSite = SameSiteMode.None, // 🔥 cross site fix
                Secure = false,            // dev mode
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return guestId;
        }

        private (int? userId, string guestId) GetIdentity()
        {
            var userId = GetUserIdFromToken();

            if (userId.HasValue)
                return (userId, null);

            return (null, GetGuestId());
        }

        // ===================== ADD TO CART =====================
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            var (userId, guestId) = GetIdentity();

            var result = await _businessLayer.AddToCart(userId, guestId, productId);

            var items = await _businessLayer.GetCart(userId, guestId);

            return Ok(new
            {
                status = true,
                message = "Product added",
                userId,
                guestId,
                totalItems = items.Count,
                grandTotal = items.Sum(x => x.totalprice),
                data = items
            });
        }

        // ===================== GET CART =====================
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var (userId, guestId) = GetIdentity();

            var items = await _businessLayer.GetCart(userId, guestId);

            return Ok(new
            {
                status = true,
                userId,
                guestId,
                totalItems = items.Count,
                grandTotal = items.Sum(x => x.totalprice),
                data = items
            });
        }
    }
}