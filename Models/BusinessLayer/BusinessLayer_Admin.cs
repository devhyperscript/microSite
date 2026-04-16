using firstproject.Models.DatabaseLayer;

namespace firstproject.Models.BusinessLayer
{
  
    public partial interface IBusinessLayer
    {
        Task<List<AdminModel>> GetAllAdmins();
    }

    public partial class  BusinessLayer : IBusinessLayer {

        public async Task<List<AdminModel>> GetAllAdmins()
        {
            var result =  await _databaseLayer.GetAllAdmins();
            return result;
        }   
    }
    

}