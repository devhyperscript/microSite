using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<categoryModel>>GetAllCategory();
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<categoryModel>> GetAllCategory()
        {
            return await _databaseLayer.GetAllCategory();
        }
    }
}
