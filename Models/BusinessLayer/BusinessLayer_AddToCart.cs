using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string? ipAddress, int? productId, int? variantId = null);
        Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null);
        Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null);

        Task MergeGuestCart(int userId, string ipAddress);

        Task<IActionResult> DeleteCartItem(int id);
        Task<IActionResult> ClearCart(int? userId, string? ipAddress);
    }

    public partial class BusinessLayer : IBusinessLayer
    {

        // =========================
        // ✅ GET CART
        // =========================
        public async Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress)
        {
            // 🔥 LOGIN USER
            if (userId.HasValue)
            {
                // ✅ Merge only if guest cart exists
                if (!string.IsNullOrEmpty(ipAddress))
                    await _databaseLayer.MergeGuestCart(userId.Value, ipAddress);

                return await _databaseLayer.GetCart(userId.Value, null);
            }

            // 🔥 GUEST USER
            if (string.IsNullOrEmpty(ipAddress))
                return new List<CartItemModel>();

            return await _databaseLayer.GetCart(null, ipAddress);
        }

        // =========================
        // ✅ ADD TO CART
        // =========================
        public async Task<string> AddToCart(int? userId, string? ipAddress, int? productId, int? variantId = null)
        {
            // 🔥 Merge before add (login case)
            if (userId.HasValue && !string.IsNullOrEmpty(ipAddress))
                await _databaseLayer.MergeGuestCart(userId.Value, ipAddress);

            // ✅ validation
            if (!productId.HasValue && !variantId.HasValue)
                return "InvalidRequest";

            return await _databaseLayer.AddToCart(userId, ipAddress, productId, variantId);
        }

        // =========================
        // ✅ ADD MULTIPLE (SAFE LOOP)
        // =========================
        public async Task<string> AddMultipleToCart(int? userId, string? ipAddress, List<int> productIds, int? variantId = null)
        {
            // ✅ login case → merge guest cart
            if (userId.HasValue && !string.IsNullOrEmpty(ipAddress))
                await MergeGuestCart(userId.Value, ipAddress);

            return await _databaseLayer.AddMultipleToCart(userId, ipAddress, productIds, variantId);
        }

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        public async Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change, int? variantId = null)
        {
            if (productId <= 0)
                return "InvalidProduct";

            if (change != 1 && change != -1)
                return "InvalidChange";

            return await _databaseLayer.UpdateCartQuantity(userId, ipAddress, productId, change, variantId);
        }

        // =========================
        // ✅ MERGE GUEST CART → USER
        // =========================
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return;

            await _databaseLayer.MergeGuestCart(userId, ipAddress);
        }

        // =========================
        // ✅ DELETE ITEM
        // =========================
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            if (id <= 0)
                return new BadRequestObjectResult(new { status = false, message = "Invalid cart id" });

            return await _databaseLayer.DeleteCartItem(id);
        }

        // =========================
        // ✅ CLEAR CART
        // =========================
        public async Task<IActionResult> ClearCart(int? userId, string? ipAddress)
        {
            if (!userId.HasValue && string.IsNullOrEmpty(ipAddress))
                return new BadRequestObjectResult(new { status = false, message = "User or Guest required" });

            return await _databaseLayer.ClearCart(userId, ipAddress);
        }
    }
}