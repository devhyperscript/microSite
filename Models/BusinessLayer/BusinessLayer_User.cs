using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
   public partial interface IBusinessLayer
    {
        Task<List<Usermodel>> GetUsers();
       


    }
    public partial class BusinessLayer : IBusinessLayer
    {



        public async Task<List<Usermodel>> GetUsers()
        {
            var result = await _databaseLayer.GetUsers();
            return result;
        }




    }
        
}
