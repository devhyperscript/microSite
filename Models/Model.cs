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
    }
      public class Size
    {
        public int Id { get; set; }

        public string SizeName { get; set; }   // S, M, L, XL, 42, 44

        public string Description { get; set; } // Optional

        public bool IsActive { get; set; } = true;
    }
}
