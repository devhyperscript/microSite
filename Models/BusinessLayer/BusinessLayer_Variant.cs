using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<Variantmodel>> GetVariant();


        Task<IActionResult> AddVariant([FromForm] Variantmodel variant);
    }   

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Variantmodel>> GetVariant()
        {
            var result = await _databaseLayer.GetVariant();
            return result;
        }

        public async Task<IActionResult> AddVariant([FromForm] Variantmodel variant)
        {
            var result = await _databaseLayer.AddVariant(variant);
            return result;
        }
    }
}
