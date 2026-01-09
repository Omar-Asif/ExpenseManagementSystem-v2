using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementSystem.ViewModels.Income
{
    public class IncomeCreateViewModel
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

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public class IncomeEditViewModel : IncomeCreateViewModel
    {
        public int Id { get; set; }
    }

    public class IncomeListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }

    public class IncomeIndexViewModel
    {
        public List<IncomeListViewModel> Incomes { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public int? FilterMonth { get; set; }
        public int? FilterYear { get; set; }
    }

    /// <summary>
    /// Custom validation attribute to prevent future dates
    /// </summary>
    public class FutureDateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                return date.Date <= DateTime.Today;
            }
            return true;
        }
    }
}
