using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
   public partial interface IBusinessLayer
    {
        Task<List<Brandmodel>> GetBrand();
        //Task<List<Brand>> GetBrands();
        Task<Brandmodel> Add(Brandmodel model);
       
    }
    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Brandmodel>> GetBrand()
        {
            var result = await _databaseLayer.GetBrand();
            return result;
        }

        public async Task<Brandmodel> Add(Brandmodel model)
        {
            return await _databaseLayer.Add(model);
        }




    }
}
