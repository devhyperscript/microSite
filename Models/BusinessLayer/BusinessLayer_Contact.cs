using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<Contactmodel>> GetContact();

        Task<IActionResult> AddContact([FromForm] Contactmodel model);
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Contactmodel>> GetContact()
        {
            var result = await _databaseLayer.GetContact();
            return result;
        }

       public async Task<IActionResult> AddContact([FromForm] Contactmodel model)
        {
            var result = await _databaseLayer.AddContact(model);
            return result;
        }
    }

}