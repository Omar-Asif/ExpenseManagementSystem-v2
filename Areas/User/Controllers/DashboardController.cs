using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class DashboardController : Controller
    {
        private readonly ExpenseDbContext _context;

        public DashboardController(ExpenseDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var lastMonth = DateTime.Today.AddMonths(-1);

            // Current month totals
            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var totalExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            // Last month totals for comparison
            var lastMonthIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == lastMonth.Month && i.Date.Year == lastMonth.Year)
                .SumAsync(i => i.Amount);

            var lastMonthExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == lastMonth.Month && e.Date.Year == lastMonth.Year)
                .SumAsync(e => e.Amount);

            // Budget totals
            var totalBudget = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear)
                .SumAsync(b => b.PlannedAmount);

            // Transaction count
            var incomeCount = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .CountAsync();

            var expenseCount = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .CountAsync();

            // Categories used this month
            var categoriesUsed = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Select(e => e.Category)
                .Distinct()
                .CountAsync();

            // Recent transactions (last 10)
            var recentIncomes = await _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ThenByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new RecentTransactionViewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Amount = i.Amount,
                    Date = i.Date,
                    Type = "Income",
                    Category = null
                })
                .ToListAsync();

            var recentExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new RecentTransactionViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Date = e.Date,
                    Type = "Expense",
                    Category = e.Category
                })
                .ToListAsync();

            var recentTransactions = recentIncomes
                .Concat(recentExpenses)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            var viewModel = new UserDashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                LastMonthIncome = lastMonthIncome,
                LastMonthExpense = lastMonthExpense,
                TotalBudget = totalBudget,
                BudgetUsed = totalExpense,
                TransactionCount = incomeCount + expenseCount,
                CategoriesUsed = categoriesUsed,
                RecentTransactions = recentTransactions,
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth)
            };

            return View(viewModel);
        }
    }
}
