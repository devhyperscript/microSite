using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
  
    public partial interface IBusinessLayer
    {
        Task<List<AdminModel>> GetAllAdmins();
        Task<IActionResult> Add([FromForm] AdminModel model);
    }

    public partial class  BusinessLayer : IBusinessLayer {

        public async Task<List<AdminModel>> GetAllAdmins()
        {
            var result =  await _databaseLayer.GetAllAdmins();
            return result;
        }   

        public async Task<IActionResult> Add([FromForm] AdminModel model)
        {
            var result = await _databaseLayer.Add(model);
            return result;
        }
    }
    

}