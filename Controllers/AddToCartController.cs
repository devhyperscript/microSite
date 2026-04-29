using firstproject.Helpers;
using firstproject.Models;
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

        // ✅ Helper: Request se IP address nikalo (dynamic system IP)
        private string GetClientIp()
        {
            // X-Forwarded-For header check karo (proxy/load balancer ke peeche)
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
                return forwarded.Split(',')[0].Trim();

            // Direct connection IP
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        // ✅ Helper: Token se userId nikalo (null if not logged in)
        private int? GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader.Substring("Bearer ".Length).Trim();
            return _jwtHelper.GetUserIdFromToken(token); // returns null if invalid
        }

        // POST api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] int productId)
        {
            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIp();

            var result = await _businessLayer.AddToCart(userId, ipAddress, productId);

            return Ok(new
            {
                status = true,
                message = "Product cart mein add ho gaya",
                addedBy = userId.HasValue ? $"userId: {userId}" : $"ip: {ipAddress}",
                data = result
            });
        }

        // GET api/cart/get
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            int? userId = GetUserIdFromToken();
            string ipAddress = GetClientIp();

            var result = await _businessLayer.GetCart(userId, ipAddress);

            return Ok(new
            {
                status = true,
                fetchedBy = userId.HasValue ? $"userId: {userId}" : $"ip: {ipAddress}",
                data = result
            });
        }


        [HttpDelete("delete/{id}")]

        //public async Task<IActionResult> DeleteCartItem(int id)
        //{
        //    int? userId = GetUserIdFromToken();
        //    string ipAddress = GetClientIp();
        //    var result = await _businessLayer.DeleteCartItem(int, id);
        //    return Ok(new
        //    {
        //        status = true,
        //        message = "Cart item delete ho gaya",
        //        deletedBy = userId.HasValue ? $"userId: {userId}" : $"ip: {ipAddress}",
        //        data = result
        //    });
        //}

        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var result = await _businessLayer.DeleteCartItem(id);
            return Ok(
                new
                {
                    status = true,
                    message = "Cart delete successful",
                    data = result
                }

                );
        }
    }
}