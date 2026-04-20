using Microsoft.AspNetCore.Mvc;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {
        Task<List<Brandmodel>> GetBrand();
        Task<Brandmodel> Add(Brandmodel model);
        Task<IActionResult> Edit(int id, [FromForm] Brandmodel model);
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
                // Step 1: Purani image path fetch karo
                var brands = await _databaseLayer.GetBrand();
                var existingBrand = brands.FirstOrDefault(b => b.Id == id);

                // Step 2: Purani image delete karo
                if (existingBrand?.BrandImage != null)
                {
                    var oldImagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existingBrand.BrandImage.TrimStart('/')
                    );

                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }

                // Step 3: Nayi image save karo
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

            // Step 4: Database update karo
            return await _databaseLayer.Edit(id, model);
        }

        public async Task<IActionResult> DeleteBrand(int id)
        {
            // Step 1: Pehle image path fetch karo
            var brands = await _databaseLayer.GetBrand();
            var brand = brands.FirstOrDefault(b => b.Id == id);

            // Step 2: Database se delete karo
            var result = await _databaseLayer.DeleteBrand(id);

            // Step 3: Agar delete successful hua to image bhi delete karo
            if (result is OkResult && brand?.BrandImage != null)
            {
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    brand.BrandImage.TrimStart('/')
                );

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            return result;
        }
    }
}