using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManagementSystem.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlannedAmount { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}