using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementSystem.ViewModels.Admin
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
    }

    public class UserIndexViewModel
    {
        public List<UserListViewModel> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // "all", "active", "inactive"
    }

    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }

        // Financial Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        
        // Activity Stats
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }
        public int BudgetCount { get; set; }
        public int TotalTransactions => IncomeCount + ExpenseCount;

        // Monthly Data
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpense { get; set; }
        
        // Recent Activity
        public List<UserTransactionViewModel> RecentTransactions { get; set; } = new();
    }

    public class UserTransactionViewModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public string? Category { get; set; }
    }

    public class ToggleUserStatusViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool NewStatus { get; set; }
    }
}
