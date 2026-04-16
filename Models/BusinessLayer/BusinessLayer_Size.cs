using Microsoft.AspNetCore.Mvc;


namespace firstproject.Models.BusinessLayer

{
    public partial interface IBusinessLayer
    {
        Task<List<Size>> GetSize();
    }

    public partial class BusinessLayer : IBusinessLayer
    {
        public async Task<List<Size>> GetSize()
        {
            var result = await _databaseLayer.GetSize();
            return result;
        }
    }
}
