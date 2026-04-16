using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{

    public partial interface IBusinessLayer
    {
        Task<List<AdminModel>> GetAllAdmins();
        Task<AdminModel> Add(AdminModel model);
        Task<AdminModel> Edit(int id, AdminModel model);
    }

    public partial class BusinessLayer : IBusinessLayer
    {

        public async Task<List<AdminModel>> GetAllAdmins()
        {
            return await _databaseLayer.GetAllAdmins();
        }

        public async Task<AdminModel> Add(AdminModel model)
        {
            return await _databaseLayer.Add(model);
        }

        public async Task<AdminModel> Edit(int id, AdminModel model)
        {
            return await _databaseLayer.Edit(id, model); 
        }


    }
}