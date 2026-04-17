using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<categoryModel>> GetAllCategory();

        Task<categoryModel> Add(categoryModel model);
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<categoryModel>> GetAllCategory()
        {
            return await _databaseLayer.GetAllCategory();
        }

        public async Task<categoryModel> Add(categoryModel model)
        {
            return await _databaseLayer.Add(model);
        }
    }
}