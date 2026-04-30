
using Microsoft.AspNetCore.Http;

namespace firstproject.Models




{
    public class AdminModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Token { get; set; }
    }

    public class AdminLoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class Size
    {
        public int Id { get; set; }

        public string SizeName { get; set; }   // S, M, L, XL, 42, 44

        public string Description { get; set; } // Optional

        public bool IsActive { get; set; } = true;
    }

    public class categoryModel
    {
        public int id { get; set; }

        public string Name { get; set; }

        public bool Status { get; set; }

        // ✅ DB ke liye (nullable)
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        // ✅ Image upload ke liye (IMPORTANT)
        public IFormFile? ImageFile { get; set; }
    }

    public class SubCategoryModel
    {
        public int Id { get; set; }

        public string? SubCategoryName { get; set; }

        public string? SubCategoryImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool Status { get; set; } = true;

        public IFormFile? ImageFile { get; set; }
    }

    public class childCategoryModel
    {
        public int Id { get; set; }
        public string? ChildCategoryName { get; set; }
        public string? ChildCategoryImageUrl { get; set; }
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; } = true;
        public IFormFile? ImageFile { get; set; }
    }



    public class Brandmodel
    {
        public int Id { get; set; }
        public string? BrandName { get; set; }
       
        public string? BrandImage { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class customermodel
    {
        public int id { get; set; }
        public string? customername { get; set; }
        public string? customerimage { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool status { get; set; } = true;

        public DateTime createdat { get; set; }
    }

    public class Colormodel
    {
        public int Id { get; set; }
        public string? Colorname { get; set; }
        public string? Colorcode { get; set; }
        public bool Isactive { get; set; } = true;
    }


    public class Usermodel
    {
        public int Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool Isactive { get; set; } = true;
        public string? Token { get; set; }
        public DateTime Createdat { get; set; }
    }

    public class UserLoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }


    public class Contactmodel 
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Subject { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }

    public class Productmodel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Sku { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int? ChildCategoryId { get; set; }
        public int? BrandId { get; set; }
        public int[]? SizeIds { get; set; }
        public int[]? ColorIds { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile[]? GalleryFiles { get; set; }
        public string? Image { get; set; }
        public string[]? ImageGallery { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ Name fields add karo
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? ChildCategoryName { get; set; }
        public string? BrandName { get; set; }
        public List<string>? SizeNames { get; set; }
        public List<string>? ColorNames { get; set; }
    }




    public class Variantmodel
    {
        public int Id { get; set; }
        public string? VariantName { get; set; }

        public int ProductId { get; set; }
        public int[]? SizeId { get; set; }
        public int[]? ColorId { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }

        public string? Sku { get; set; }

        // Images
        public IFormFile? ImageFile { get; set; }
        public IFormFile[]? GalleryFiles { get; set; }

        public string? Image { get; set; }
        public string[]? ImageGallery { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // For joins (optional)
        public string[]? SizeNames { get; set; }
        public string[]? ColorNames { get; set; }
    }

    public class CartItemModel
    {
        // 🔑 Cart Info
        public int id { get; set; }
        public int? userid { get; set; }      // nullable (guest ke liye)
        public string? ipaddress { get; set; } // guest user ke liye

        public int productid { get; set; }
        public int quantity { get; set; } = 1;

        // 🛍 Product Info (JOIN se aayega)
        public string? ProductName { get; set; }
        public string? Slug { get; set; }
        public string? Image { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        // 💰 Calculated
        public decimal totalprice { get; set; }

        // 📅 Meta
        public DateTime? createdat { get; set; }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public string? ipaddress { get; set; }
    }

    /// <summary>Raw row for <c>cart_items</c> (IP-based guest or logged-in user id).</summary>
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? ClientIp { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
