using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
   public partial interface IBusinessLayer
    {
        Task<List<Brandmodel>> GetBrand();
     
        Task<Brandmodel> Add(Brandmodel model);
        Task<IActionResult> Edit(int id, [FromForm] Brandmodel model);

        //Task<IActionResult> EditSize(int id, [FromForm] Size size);

        Task<IActionResult> DeleteBrand(int id);

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
        public async Task<IActionResult> Edit(int id, [FromForm] Brandmodel model)
        {
            if (model.ImageFile != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                model.BrandImage = "/uploads/" + fileName;
            }
            return await _databaseLayer.Edit(id, model);
        }

        public async Task<IActionResult> DeleteBrand(int id)
        {
            var result = await _databaseLayer.DeleteBrand(id);
            return result;

        }




    }
}
