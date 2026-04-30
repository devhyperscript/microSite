using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? ipAddress);
        Task<string> AddToCart(int? userId, string? ipAddress, int productId);
        Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change);
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
            if (userId.HasValue)
            {
                // 🔥 Merge only if IP exists
                if (!string.IsNullOrEmpty(ipAddress))
                    await MergeGuestCart(userId.Value, ipAddress);

                return await _databaseLayer.GetCart(userId.Value, null);
            }
            else
            {
                // Guest user → IP se fetch
                if (string.IsNullOrEmpty(ipAddress))
                    return new List<CartItemModel>();

                return await _databaseLayer.GetCart(null, ipAddress);
            }
        }

        // =========================
        // ✅ ADD TO CART
        // =========================
        public async Task<string> AddToCart(int? userId, string? ipAddress, int productId)
        {
            return await _databaseLayer.AddToCart(userId, ipAddress, productId);
        }

        // =========================
        // ✅ UPDATE QUANTITY
        // =========================
        public async Task<string> UpdateCartQuantity(int? userId, string? ipAddress, int productId, int change)
        {
            return await _databaseLayer.UpdateCartQuantity(userId, ipAddress, productId, change);
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
            return await _databaseLayer.DeleteCartItem(id);
        }

        // =========================
        // ✅ CLEAR CART
        // =========================
        public async Task<IActionResult> ClearCart(int? userId, string? ipAddress)
        {
            return await _databaseLayer.ClearCart(userId, ipAddress);
        }
    }
}