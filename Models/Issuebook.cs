using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class IssueBook
    {
        [Key]
        public int IssueId { get; set; }

        // ===========================
        // ISSUE TYPE
        // ===========================
        [Required]
        [StringLength(20)]
        public string IssueType { get; set; } = "Student";

        // ===========================
        // STUDENT (Nullable)
        // ===========================
        public int? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        // ===========================
        // TEACHER (Nullable)
        // ===========================
        public int? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        // ===========================
        // BOOK
        // ===========================
        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        // ===========================
        // DATES
        // ===========================
        [Required]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        // ===========================
        // FINE
        // ===========================
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fine { get; set; } = 0;

        // ===========================
        // STATUS
        // ===========================
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Issued";
    }
}