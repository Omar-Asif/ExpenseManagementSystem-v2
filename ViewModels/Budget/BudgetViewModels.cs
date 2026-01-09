using System.ComponentModel.DataAnnotations;
using ExpenseManagementSystem.ViewModels.Income;

namespace ExpenseManagementSystem.ViewModels.Budget
{
    public class BudgetCreateViewModel
    {
        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Planned amount is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Planned Amount")]
        public decimal PlannedAmount { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
        [Display(Name = "Year")]
        [FutureBudgetValidation(ErrorMessage = "Cannot create budget for future months beyond current month")]
        public int Year { get; set; } = DateTime.Today.Year;
    }

    public class BudgetEditViewModel : BudgetCreateViewModel
    {
        public int Id { get; set; }
    }

    public class BudgetListViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal PlannedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => PlannedAmount - SpentAmount;
        public double PercentageUsed => PlannedAmount > 0 ? (double)(SpentAmount / PlannedAmount) * 100 : 0;
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsOverBudget => SpentAmount > PlannedAmount;
    }

    public class BudgetIndexViewModel
    {
        public List<BudgetListViewModel> Budgets { get; set; } = new();
        public decimal TotalPlanned { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining => TotalPlanned - TotalSpent;
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public int FilterMonth { get; set; }
        public int FilterYear { get; set; }
    }

    /// <summary>
    /// Custom validation to prevent budgets for future months
    /// </summary>
    public class FutureBudgetValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;
            var monthProperty = model.GetType().GetProperty("Month");
            var yearProperty = model.GetType().GetProperty("Year");

            if (monthProperty != null && yearProperty != null)
            {
                var month = (int)(monthProperty.GetValue(model) ?? 0);
                var year = (int)(yearProperty.GetValue(model) ?? 0);

                var budgetDate = new DateTime(year, month, 1);
                var currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                // Allow current month and past months only
                if (budgetDate > currentDate)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
