namespace ExpenseManagementSystem.ViewModels.Analytics
{
    public class UserAnalyticsViewModel
    {
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;

        // Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        public double SavingsRate => TotalIncome > 0 
            ? ((double)Balance / (double)TotalIncome) * 100 
            : 0;

        // Comparison
        public decimal LastMonthIncome { get; set; }
        public decimal LastMonthExpense { get; set; }
        public double IncomeGrowthRate => LastMonthIncome > 0 
            ? ((double)(TotalIncome - LastMonthIncome) / (double)LastMonthIncome) * 100 
            : (TotalIncome > 0 ? 100 : 0);
        public double ExpenseGrowthRate => LastMonthExpense > 0 
            ? ((double)(TotalExpense - LastMonthExpense) / (double)LastMonthExpense) * 100 
            : (TotalExpense > 0 ? 100 : 0);

        // Average Daily
        public decimal AverageDailyIncome { get; set; }
        public decimal AverageDailyExpense { get; set; }

        // Budget Analysis
        public decimal TotalBudget { get; set; }
        public decimal BudgetSpent { get; set; }
        public double BudgetUtilization => TotalBudget > 0 
            ? ((double)BudgetSpent / (double)TotalBudget) * 100 
            : 0;
        public int BudgetsOnTrack { get; set; }
        public int BudgetsOverspent { get; set; }

        // Trends
        public List<DailyTrendViewModel> DailyTrends { get; set; } = new();
        public List<MonthlyTrendItemViewModel> MonthlyTrends { get; set; } = new();
        public List<CategoryTrendViewModel> CategoryTrends { get; set; } = new();

        // Insights
        public List<FinancialInsightViewModel> Insights { get; set; } = new();
    }

    public class DailyTrendViewModel
    {
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance => Income - Expense;
    }

    public class MonthlyTrendItemViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance => Income - Expense;
        public double SavingsRate => Income > 0 ? ((double)Balance / (double)Income) * 100 : 0;
    }

    public class CategoryTrendViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal CurrentMonth { get; set; }
        public decimal LastMonth { get; set; }
        public double ChangePercentage => LastMonth > 0 
            ? ((double)(CurrentMonth - LastMonth) / (double)LastMonth) * 100 
            : (CurrentMonth > 0 ? 100 : 0);
        public bool IsIncreased => CurrentMonth > LastMonth;
    }

    public class FinancialInsightViewModel
    {
        public string Type { get; set; } = string.Empty; // "success", "warning", "danger", "info"
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
