namespace ExpenseManagementSystem.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        
        public decimal TotalSystemIncome { get; set; }
        public decimal TotalSystemExpense { get; set; }
        public decimal TotalSystemBalance => TotalSystemIncome - TotalSystemExpense;
        
        public int TotalTransactions { get; set; }
        public int TotalBudgets { get; set; }
        
        public List<RecentUserViewModel> RecentUsers { get; set; } = new();
        public List<UserActivityViewModel> TopActiveUsers { get; set; } = new();
        
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;
    }

    public class RecentUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserActivityViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
