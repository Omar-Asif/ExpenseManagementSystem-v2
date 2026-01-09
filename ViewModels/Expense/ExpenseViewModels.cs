using System.ComponentModel.DataAnnotations;
using ExpenseManagementSystem.ViewModels.Income;

namespace ExpenseManagementSystem.ViewModels.Expense
{
    public class ExpenseCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        [FutureDateValidation(ErrorMessage = "Date cannot be in the future")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public class ExpenseEditViewModel : ExpenseCreateViewModel
    {
        public int Id { get; set; }
    }

    public class ExpenseListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ExpenseIndexViewModel
    {
        public List<ExpenseListViewModel> Expenses { get; set; } = new();
        public decimal TotalExpense { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public int? FilterMonth { get; set; }
        public int? FilterYear { get; set; }
        public string? FilterCategory { get; set; }
        public List<string> Categories { get; set; } = new();
    }

    /// <summary>
    /// Predefined expense categories
    /// </summary>
    public static class ExpenseCategories
    {
        public static readonly List<string> Categories = new()
        {
            "Food & Dining",
            "Transportation",
            "Shopping",
            "Entertainment",
            "Bills & Utilities",
            "Health & Medical",
            "Education",
            "Travel",
            "Personal Care",
            "Home & Rent",
            "Insurance",
            "Savings & Investments",
            "Gifts & Donations",
            "Other"
        };
    }
}
