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

        // ✅ Dynamic IP Address (REAL)
        private string GetClientIpAddress()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            return ip ?? "0.0.0.0";
        }

        // ✅ JWT se userId nikalo
        private int? GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return _jwtHelper.GetUserIdFromToken(
                authHeader.Substring("Bearer ".Length).Trim()
            );
        }

        // =========================
        // ✅ ADD TO CART
        // =========================
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIpAddress();

            var result = await _businessLayer.AddToCart(userId, ipAddress, productId);

            if (result == "AlreadyInCart")
                return Ok(new { status = false, message = "Product already cart mein hai" });

            var items = await _businessLayer.GetCart(userId, ipAddress);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Product cart mein add ho gaya",
                userId = userId,
                ipAddress = userId == null ? ipAddress : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        // =========================
        // ✅ GET CART
        // =========================
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIpAddress();

            var items = await _businessLayer.GetCart(userId, ipAddress);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                userId = userId,
                ipAddress = userId == null ? ipAddress : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        [HttpPut("updatequantity")]
        public async Task<IActionResult> UpdateQuantity([FromForm] int productId, [FromForm] int change)
        {
            if (change != 1 && change != -1)
                return BadRequest(new { status = false, message = "change sirf +1 ya -1 hona chahiye" });

            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIpAddress();

            await _businessLayer.UpdateCartQuantity(userId, ipAddress, productId, change);

            var items = await _businessLayer.GetCart(userId, ipAddress);
            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = change == 1 ? "Quantity badh gayi (+1)" : "Quantity kam ho gayi (-1)",
                userId = userId,
                ipAddress = userId == null ? ipAddress : null,
                totalItems = items.Count,
                grandTotal = grandTotal,
                data = items
            });
        }

        // =========================
        // ✅ DELETE ITEM
        // =========================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _businessLayer.DeleteCartItem(id);
        }

        // =========================
        // ✅ CLEAR CART
        // =========================
        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIpAddress();

            return await _businessLayer.ClearCart(userId, ipAddress);
        }
    }
}