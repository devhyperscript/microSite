using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string? guestId);
        Task<string> AddToCart(int? userId, string? guestId, int productId);
        Task<string> UpdateCartQuantity(int? userId, string? guestId, int productId, int change);
        Task MergeGuestCart(int userId, string guestId);
        Task<IActionResult> DeleteCartItem(int id);
        Task<IActionResult> ClearCart(int? userId, string? guestId);
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<CartItemModel>> GetCart(int? userId, string? guestId)
        {
            if (userId.HasValue)
            {
                if (!string.IsNullOrEmpty(guestId))
                    await MergeGuestCart(userId.Value, guestId);

                return await _databaseLayer.GetCart(userId.Value, null);
            }
            else
            {
                if (string.IsNullOrEmpty(guestId))
                    return new List<CartItemModel>();

                return await _databaseLayer.GetCart(null, guestId);
            }
        }

        public async Task<string> AddToCart(int? userId, string? guestId, int productId)
        {
            return await _databaseLayer.AddToCart(userId, guestId, productId);
        }

        public async Task<string> UpdateCartQuantity(int? userId, string? guestId, int productId, int change)
        {
            return await _databaseLayer.UpdateCartQuantity(userId, guestId, productId, change);
        }

        public async Task MergeGuestCart(int userId, string guestId)
        {
            if (string.IsNullOrEmpty(guestId))
                return;

            await _databaseLayer.MergeGuestCart(userId, guestId);
        }

        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _databaseLayer.DeleteCartItem(id);
        }

        public async Task<IActionResult> ClearCart(int? userId, string? guestId)
        {
            return await _databaseLayer.ClearCart(userId, guestId);
        }
    }
}