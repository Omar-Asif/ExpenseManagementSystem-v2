namespace ExpenseManagementSystem.ViewModels.Dashboard
{
    public class UserDashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal RemainingBalance => TotalIncome - TotalExpense;
        
        public decimal LastMonthIncome { get; set; }
        public decimal LastMonthExpense { get; set; }
        
        public double IncomeChangePercentage => LastMonthIncome > 0 
            ? ((double)(TotalIncome - LastMonthIncome) / (double)LastMonthIncome) * 100 
            : (TotalIncome > 0 ? 100 : 0);
            
        public double ExpenseChangePercentage => LastMonthExpense > 0 
            ? ((double)(TotalExpense - LastMonthExpense) / (double)LastMonthExpense) * 100 
            : (TotalExpense > 0 ? 100 : 0);

        public decimal TotalBudget { get; set; }
        public decimal BudgetUsed { get; set; }
        public double BudgetUsedPercentage => TotalBudget > 0 
            ? ((double)BudgetUsed / (double)TotalBudget) * 100 
            : 0;

        public int TransactionCount { get; set; }
        public int CategoriesUsed { get; set; }
        public double SavingsRate => TotalIncome > 0 
            ? ((double)RemainingBalance / (double)TotalIncome) * 100 
            : 0;

        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
        
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;
    }

    public class RecentTransactionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public string? Category { get; set; }
    }
}
