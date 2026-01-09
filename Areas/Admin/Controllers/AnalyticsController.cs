using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _applicationContext;
        private readonly ExpenseDbContext _expenseContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalyticsController(
            ApplicationDbContext applicationContext,
            ExpenseDbContext expenseContext,
            UserManager<ApplicationUser> userManager)
        {
            _applicationContext = applicationContext;
            _expenseContext = expenseContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var lastMonth = DateTime.Today.AddMonths(-1);

            // User Statistics
            var allUsers = await _userManager.GetUsersInRoleAsync(Roles.User);
            var usersList = allUsers.ToList();
            var totalUsers = usersList.Count;
            var activeUsers = usersList.Count(u => u.IsActive);
            var newUsersThisMonth = usersList.Count(u => 
                u.CreatedAt.Month == currentMonth && u.CreatedAt.Year == currentYear);
            var lastMonthUsers = usersList.Count(u => 
                u.CreatedAt.Month == lastMonth.Month && u.CreatedAt.Year == lastMonth.Year);
            var userGrowthRate = lastMonthUsers > 0 
                ? ((double)(newUsersThisMonth - lastMonthUsers) / lastMonthUsers) * 100 
                : (newUsersThisMonth > 0 ? 100 : 0);

            // Total Financial Statistics
            var totalIncome = await _expenseContext.Incomes.SumAsync(i => i.Amount);
            var totalExpense = await _expenseContext.Expenses.SumAsync(e => e.Amount);

            // Monthly Financial Comparison
            var currentMonthIncome = await _expenseContext.Incomes
                .Where(i => i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var currentMonthExpense = await _expenseContext.Expenses
                .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            var lastMonthIncome = await _expenseContext.Incomes
                .Where(i => i.Date.Month == lastMonth.Month && i.Date.Year == lastMonth.Year)
                .SumAsync(i => i.Amount);

            var lastMonthExpense = await _expenseContext.Expenses
                .Where(e => e.Date.Month == lastMonth.Month && e.Date.Year == lastMonth.Year)
                .SumAsync(e => e.Amount);

            // Transaction Statistics
            var incomeTransactions = await _expenseContext.Incomes.CountAsync();
            var expenseTransactions = await _expenseContext.Expenses.CountAsync();
            var budgetsCreated = await _expenseContext.Budgets.CountAsync();

            // Expense by Category
            var expenseByCategory = await _expenseContext.Expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryBreakdownViewModel
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(10)
                .ToListAsync();

            // Calculate percentages
            if (totalExpense > 0)
            {
                foreach (var category in expenseByCategory)
                {
                    category.Percentage = (double)category.TotalAmount / (double)totalExpense * 100;
                }
            }

            // Top Users by Income
            var userIds = usersList.Select(u => u.Id).ToList();
            var topUsersByIncome = await _expenseContext.Incomes
                .Where(i => userIds.Contains(i.UserId))
                .GroupBy(i => i.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Amount = g.Sum(i => i.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToListAsync();

            var topIncomeUsers = topUsersByIncome.Select(t =>
            {
                var user = usersList.FirstOrDefault(u => u.Id == t.UserId);
                return new TopUserViewModel
                {
                    UserId = t.UserId,
                    FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    Email = user?.Email ?? string.Empty,
                    Amount = t.Amount,
                    TransactionCount = t.Count
                };
            }).ToList();

            // Top Users by Expense
            var topUsersByExpense = await _expenseContext.Expenses
                .Where(e => userIds.Contains(e.UserId))
                .GroupBy(e => e.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToListAsync();

            var topExpenseUsers = topUsersByExpense.Select(t =>
            {
                var user = usersList.FirstOrDefault(u => u.Id == t.UserId);
                return new TopUserViewModel
                {
                    UserId = t.UserId,
                    FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    Email = user?.Email ?? string.Empty,
                    Amount = t.Amount,
                    TransactionCount = t.Count
                };
            }).ToList();

            // Monthly Trends (last 6 months)
            var monthlyTrends = new List<MonthlyTrendViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Today.AddMonths(-i);
                var monthIncome = await _expenseContext.Incomes
                    .Where(x => x.Date.Month == date.Month && x.Date.Year == date.Year)
                    .SumAsync(x => x.Amount);
                var monthExpense = await _expenseContext.Expenses
                    .Where(x => x.Date.Month == date.Month && x.Date.Year == date.Year)
                    .SumAsync(x => x.Amount);
                var monthNewUsers = usersList.Count(u => 
                    u.CreatedAt.Month == date.Month && u.CreatedAt.Year == date.Year);

                monthlyTrends.Add(new MonthlyTrendViewModel
                {
                    Month = date.Month,
                    Year = date.Year,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(date.Month),
                    TotalIncome = monthIncome,
                    TotalExpense = monthExpense,
                    NewUsers = monthNewUsers
                });
            }

            var viewModel = new AdminAnalyticsViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersThisMonth = newUsersThisMonth,
                UserGrowthRate = userGrowthRate,
                TotalSystemIncome = totalIncome,
                TotalSystemExpense = totalExpense,
                CurrentMonthIncome = currentMonthIncome,
                CurrentMonthExpense = currentMonthExpense,
                LastMonthIncome = lastMonthIncome,
                LastMonthExpense = lastMonthExpense,
                TotalTransactions = incomeTransactions + expenseTransactions,
                IncomeTransactions = incomeTransactions,
                ExpenseTransactions = expenseTransactions,
                BudgetsCreated = budgetsCreated,
                ExpenseByCategory = expenseByCategory,
                TopUsersByIncome = topIncomeUsers,
                TopUsersByExpense = topExpenseUsers,
                MonthlyTrends = monthlyTrends,
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth)
            };

            return View(viewModel);
        }
    }
}
