using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Edition { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Language { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Range(0, 10000)]
        public int AvailableQuantity { get; set; }

        [StringLength(20)]
        public string ShelfNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Available";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}