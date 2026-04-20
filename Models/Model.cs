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

        public string Token { get; set; }
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




    public class Brandmodel
    {
        public int Id { get; set; }
        public string? BrandName { get; set; }
       
        public string? BrandImage { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsActive { get; set; } = true;
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
}
