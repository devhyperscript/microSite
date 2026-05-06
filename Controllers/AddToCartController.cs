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

        // ===================== AUTH =====================

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
        public async Task<IActionResult> AddToCart(
            [FromForm] int? productId = null,
            [FromForm] int? variantId = null)
        {
            if (!productId.HasValue && !variantId.HasValue)
                return BadRequest(new { status = false, message = "productId ya variantId zaroor bhejo" });

            var (userId, guestId) = GetIdentity();

            var result = await _businessLayer.AddToCart(userId, guestId, productId, variantId);

            if (result == "VariantNotFound")
                return NotFound(new { status = false, message = "Variant not found" });

            if (result == "ProductNotFound")
                return NotFound(new { status = false, message = "Product not found" });

            var items = await _businessLayer.GetCart(userId, guestId);

            var item = items.FirstOrDefault(x =>
                (variantId.HasValue && x.variantid == variantId) ||
                (!variantId.HasValue && x.productid == productId)
            );

            if (item == null)
                return Ok(new { status = true, message = "Added but item not found" });

            return Ok(new
            {
                status = true,
                message = "Added to cart",
                data = new
                {
                    id = item.id,
                    productId = item.productid,
                    variantId = item.variantid,

                    name = item.Name,
                    image = item.Image,

                    discountPrice = item.Price, // ✅ always sale price

                    quantity = item.quantity,
                    totalprice = item.totalprice
                }
            });
        }

        // ===================== GET CART =====================

        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var (userId, guestId) = GetIdentity();

            var items = await _businessLayer.GetCart(userId, guestId);

            var formatted = items.Select(x => new
            {
                id = x.id,
                productId = x.productid,
                variantId = x.variantid,

                name = x.Name,
                image = x.Image,

                discountPrice = x.Price, // ✅ final price

                quantity = x.quantity,
                totalprice = x.totalprice,

                sizeIds = x.VariantSizeIds,
                colorIds = x.VariantColorIds
            });

            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                totalItems = items.Count,
                grandTotal,
                data = formatted
            });
        }

        // ===================== UPDATE QUANTITY =====================

        [HttpPut("updatequantity")]
        public async Task<IActionResult> UpdateQuantity(
            [FromForm] int productId,
            [FromForm] int change,
            [FromForm] int? variantId = null)
        {
            if (change != 1 && change != -1)
                return BadRequest(new { status = false, message = "Invalid change value" });

            var (userId, guestId) = GetIdentity();

            await _businessLayer.UpdateCartQuantity(userId, guestId, productId, change, variantId);

            var items = await _businessLayer.GetCart(userId, guestId);

            decimal grandTotal = items.Sum(x => x.totalprice);

            return Ok(new
            {
                status = true,
                message = "Cart updated",
                totalItems = items.Count,
                grandTotal,
                data = items.Select(x => new
                {
                    id = x.id,
                    productId = x.productid,
                    variantId = x.variantid,

                    name = x.Name,
                    image = x.Image,

                    discountPrice = x.Price,

                    quantity = x.quantity,
                    totalprice = x.totalprice
                })
            });
        }

        // ===================== DELETE =====================

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _businessLayer.DeleteCartItem(id);
        }

        // ===================== CLEAR =====================

        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            var (userId, guestId) = GetIdentity();
            return await _businessLayer.ClearCart(userId, guestId);
        }

        // ===================== ADD MULTIPLE =====================

        [HttpPost("add-multiple")]
        public async Task<IActionResult> AddMultipleToCart(
            [FromForm] List<int> productIds,
            [FromForm] int? variantId = null)
        {
            if (productIds == null || !productIds.Any())
                return BadRequest(new { status = false, message = "ProductIds required" });

            var (userId, guestId) = GetIdentity();

            await _businessLayer.AddMultipleToCart(userId, guestId, productIds, variantId);

            var items = await _businessLayer.GetCart(userId, guestId);

            var products = items
                .Where(x => x.variantid == null)
                .Select(x => new
                {
                    id = x.id,
                    productId = x.productid,
                    name = x.Name,
                    image = x.Image,
                    discountPrice = x.Price,
                    quantity = x.quantity,
                    totalprice = x.totalprice
                });

            var variants = items
                .Where(x => x.variantid != null)
                .Select(x => new
                {
                    id = x.id,
                    variantId = x.variantid,
                    productId = x.productid,
                    name = x.Name,
                    image = x.Image,
                    discountPrice = x.Price,
                    sizeIds = x.VariantSizeIds,
                    colorIds = x.VariantColorIds,
                    quantity = x.quantity,
                    totalprice = x.totalprice
                });

            return Ok(new
            {
                status = true,
                totalItems = items.Count,
                grandTotal = items.Sum(x => x.totalprice),
                products,
                variants
            });
        }
    }
}