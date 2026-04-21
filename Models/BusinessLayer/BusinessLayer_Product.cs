using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
   public partial interface IBusinessLayer
    {
        Task<List<Productmodel>> GetProduct();
        Task<IActionResult> AddProduct([FromForm] Productmodel product);
        Task<Productmodel?> GetProductById(int id);
        Task<IActionResult> UpdateProduct(int id, [FromForm] Productmodel product);
        Task<IActionResult> DeleteProduct(int id);
    }
    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Productmodel>> GetProduct()
        {
            var result = await _databaseLayer.GetProduct();
            return result;
        }

       
        public async Task<IActionResult> AddProduct([FromForm] Productmodel product)
        {
            var result = await _databaseLayer.AddProduct(product);
            return result;
        }
public async Task<Productmodel?> GetProductById(int id)
        {
            var result = await _databaseLayer.GetProductById(id);
            return result;
        }
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Productmodel product)
        {
            var result = await _databaseLayer.UpdateProduct(id, product);
            return result;
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _databaseLayer.DeleteProduct(id);
            return result;
        }


    }
}
