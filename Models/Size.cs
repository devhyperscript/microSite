namespace firstproject.Models
{
    public class Size
    {
        public int Id { get; set; }

        public string SizeName { get; set; }   // S, M, L, XL, 42, 44

        public string Description { get; set; } // Optional

        public bool IsActive { get; set; } = true;
    }
}
