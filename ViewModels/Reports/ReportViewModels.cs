namespace ExpenseManagementSystem.ViewModels.Reports
{
    public class ReportIndexViewModel
    {
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;
        
        // Monthly Summary
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpense { get; set; }
        public decimal MonthlyBalance => MonthlyIncome - MonthlyExpense;
        public double MonthlySavingsRate => MonthlyIncome > 0 
            ? ((double)MonthlyBalance / (double)MonthlyIncome) * 100 
            : 0;

        // Yearly Summary
        public decimal YearlyIncome { get; set; }
        public decimal YearlyExpense { get; set; }
        public decimal YearlyBalance => YearlyIncome - YearlyExpense;
        public double YearlySavingsRate => YearlyIncome > 0 
            ? ((double)YearlyBalance / (double)YearlyIncome) * 100 
            : 0;

        // Transaction Counts
        public int MonthlyIncomeCount { get; set; }
        public int MonthlyExpenseCount { get; set; }
        public int YearlyIncomeCount { get; set; }
        public int YearlyExpenseCount { get; set; }

        // Category Breakdown for Current Month
        public List<CategorySummaryViewModel> ExpenseByCategory { get; set; } = new();
        
        // Monthly Trends for the Year
        public List<MonthlyReportViewModel> MonthlyTrends { get; set; } = new();

        // Available years for report generation
        public List<int> AvailableYears { get; set; } = new();
    }

    public class CategorySummaryViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class MonthlyReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance => Income - Expense;
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }
    }

    public class MonthlyReportDetailViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        // Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        public double SavingsRate => TotalIncome > 0 
            ? ((double)Balance / (double)TotalIncome) * 100 
            : 0;

        // Budget Summary
        public decimal TotalBudget { get; set; }
        public decimal BudgetUsed { get; set; }
        public double BudgetUsedPercentage => TotalBudget > 0 
            ? ((double)BudgetUsed / (double)TotalBudget) * 100 
            : 0;

        // Transactions
        public List<ReportIncomeViewModel> Incomes { get; set; } = new();
        public List<ReportExpenseViewModel> Expenses { get; set; } = new();
        public List<CategorySummaryViewModel> ExpenseByCategory { get; set; } = new();
        public List<ReportBudgetViewModel> Budgets { get; set; } = new();
    }

    public class YearlyReportDetailViewModel
    {
        public int Year { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        // Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        public double SavingsRate => TotalIncome > 0 
            ? ((double)Balance / (double)TotalIncome) * 100 
            : 0;

        // Transaction Counts
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }

        // Monthly Breakdown
        public List<MonthlyReportViewModel> MonthlyBreakdown { get; set; } = new();
        
        // Category Summary
        public List<CategorySummaryViewModel> ExpenseByCategory { get; set; } = new();
        
        // Top Income Sources
        public List<TopItemViewModel> TopIncomeSources { get; set; } = new();
        
        // Top Expense Categories
        public List<TopItemViewModel> TopExpenseCategories { get; set; } = new();
    }

    public class ReportIncomeViewModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }

    public class ReportExpenseViewModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ReportBudgetViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal PlannedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => PlannedAmount - SpentAmount;
        public double UsedPercentage => PlannedAmount > 0 
            ? ((double)SpentAmount / (double)PlannedAmount) * 100 
            : 0;
        public bool IsOverBudget => SpentAmount > PlannedAmount;
    }

    public class TopItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}
