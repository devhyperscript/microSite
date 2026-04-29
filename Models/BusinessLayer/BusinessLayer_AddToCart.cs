using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<CartItemModel>> GetCart(int? userId, string ipAddress);
        Task<string> AddToCart(int? userId, string ipAddress, int productId);
        Task MergeGuestCart(int userId, string ipAddress);

        Task<IActionResult> DeleteCartItem(int id);
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        // ✅ Login hai → userId se cart fetch karo
        // ✅ Guest hai → IP address se cart fetch karo
        public async Task<List<CartItemModel>> GetCart(int? userId, string ipAddress)
        {
            if (userId.HasValue)
                return await _databaseLayer.GetCart(userId.Value, null);       // userId se
            else
                return await _databaseLayer.GetCart(null, ipAddress);          // IP se
        }

        // ✅ Login hai → userId se add karo
        // ✅ Guest hai → IP address se add karo
        public async Task<string> AddToCart(int? userId, string ipAddress, int productId)
        {
            return await _databaseLayer.AddToCart(userId, ipAddress, productId);
        }

        // ✅ Login ke baad guest cart → user cart mein merge karo
        public async Task MergeGuestCart(int userId, string ipAddress)
        {
            await _databaseLayer.MergeGuestCart(userId, ipAddress);
        }

        public async Task<IActionResult> DeleteCartItem(int id)
        {
            return await _databaseLayer.DeleteCartItem(id);
        }
    }
}