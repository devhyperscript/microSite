using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
   public partial interface IBusinessLayer
    {
        Task<List<Brand>> GetBrand();
       //Task<List<Brand>> GetBrands();
        Task<IActionResult> AddBrand([FromForm] Brand brand);
        //Task<IActionResult> EditBrand(int id, [FromForm] Brand brand);
        //Task<IActionResult> DeleteBrand(int id);
    }
    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Brand>> GetBrand()
        {
            var result = await _databaseLayer.GetBrand();
            return result;
        }


        public async Task<IActionResult> AddBrand([FromForm] Brand brand)
        {
            var result = await _databaseLayer.AddBrand(brand);
            return result;
        }

        //BadImageFormatException bhi ayegi  to us accorngn kna hi hoga


    }
}
