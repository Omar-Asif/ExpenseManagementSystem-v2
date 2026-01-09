namespace ExpenseManagementSystem.ViewModels.Admin
{
    public class AdminAnalyticsViewModel
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double UserGrowthRate { get; set; }

        // Financial Statistics
        public decimal TotalSystemIncome { get; set; }
        public decimal TotalSystemExpense { get; set; }
        public decimal SystemBalance => TotalSystemIncome - TotalSystemExpense;
        
        // Monthly Comparison
        public decimal CurrentMonthIncome { get; set; }
        public decimal CurrentMonthExpense { get; set; }
        public decimal LastMonthIncome { get; set; }
        public decimal LastMonthExpense { get; set; }
        
        public double IncomeGrowthRate => LastMonthIncome > 0 
            ? ((double)(CurrentMonthIncome - LastMonthIncome) / (double)LastMonthIncome) * 100 
            : 0;
        public double ExpenseGrowthRate => LastMonthExpense > 0 
            ? ((double)(CurrentMonthExpense - LastMonthExpense) / (double)LastMonthExpense) * 100 
            : 0;

        // Transaction Statistics
        public int TotalTransactions { get; set; }
        public int IncomeTransactions { get; set; }
        public int ExpenseTransactions { get; set; }
        public int BudgetsCreated { get; set; }

        // Category Breakdown
        public List<CategoryBreakdownViewModel> ExpenseByCategory { get; set; } = new();
        
        // Top Users
        public List<TopUserViewModel> TopUsersByIncome { get; set; } = new();
        public List<TopUserViewModel> TopUsersByExpense { get; set; } = new();
        
        // Monthly Trends
        public List<MonthlyTrendViewModel> MonthlyTrends { get; set; } = new();

        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;
    }

    public class CategoryBreakdownViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
    }

    public class TopUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class MonthlyTrendViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public int NewUsers { get; set; }
    }
}
