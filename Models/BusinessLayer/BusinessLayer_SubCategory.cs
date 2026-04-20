using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer {
        Task<List<SubCategoryModel>> GetAllSubCategory();
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<SubCategoryModel>>GetAllSubCategory()
        {
            return await _databaseLayer.GetAllSubCategory();
        }
    }

}
