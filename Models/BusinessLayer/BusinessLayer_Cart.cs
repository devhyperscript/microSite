using firstproject.Models;
using firstproject.Models.DatabaseLayer;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task AddOrUpdateCartItemAsync(int? userId, string? clientIp, int productId, int quantity);
        Task<List<CartItem>> GetCartAsync(int? userId, string? clientIp);
        Task MergeGuestCartToUserAsync(string clientIp, int userId);
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public Task AddOrUpdateCartItemAsync(int? userId, string? clientIp, int productId, int quantity)
        {
            return _databaseLayer.AddOrUpdateCartItemAsync(userId, clientIp, productId, quantity);
        }

        public Task<List<CartItem>> GetCartAsync(int? userId, string? clientIp)
        {
            return _databaseLayer.GetCartAsync(userId, clientIp);
        }

        public Task MergeGuestCartToUserAsync(string clientIp, int userId)
        {
            return _databaseLayer.MergeGuestCartToUserAsync(clientIp, userId);
        }
    }
}
