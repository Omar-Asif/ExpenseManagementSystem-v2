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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _applicationContext;
        private readonly ExpenseDbContext _expenseContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
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

            // Get all users (excluding admins)
            var allUsers = await _userManager.GetUsersInRoleAsync(Roles.User);
            var usersList = allUsers.ToList();

            // User statistics
            var totalUsers = usersList.Count;
            var activeUsers = usersList.Count(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;
            var newUsersThisMonth = usersList.Count(u => 
                u.CreatedAt.Month == currentMonth && u.CreatedAt.Year == currentYear);

            // Financial statistics (system-wide)
            var totalIncome = await _expenseContext.Incomes.SumAsync(i => i.Amount);
            var totalExpense = await _expenseContext.Expenses.SumAsync(e => e.Amount);

            // Transaction counts
            var totalTransactions = await _expenseContext.Incomes.CountAsync() + 
                                   await _expenseContext.Expenses.CountAsync();
            var totalBudgets = await _expenseContext.Budgets.CountAsync();

            // Recent users (last 5)
            var recentUsers = usersList
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentUserViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email ?? string.Empty,
                    JoinedDate = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToList();

            // Top active users by transaction count
            var userIds = usersList.Select(u => u.Id).ToList();
            
            var incomesByUser = await _expenseContext.Incomes
                .Where(i => userIds.Contains(i.UserId))
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count(), Amount = g.Sum(i => i.Amount) })
                .ToListAsync();

            var expensesByUser = await _expenseContext.Expenses
                .Where(e => userIds.Contains(e.UserId))
                .GroupBy(e => e.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count(), Amount = g.Sum(e => e.Amount) })
                .ToListAsync();

            var topActiveUsers = usersList
                .Select(u => {
                    var incomeData = incomesByUser.FirstOrDefault(i => i.UserId == u.Id);
                    var expenseData = expensesByUser.FirstOrDefault(e => e.UserId == u.Id);
                    return new UserActivityViewModel
                    {
                        Id = u.Id,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Email = u.Email ?? string.Empty,
                        TransactionCount = (incomeData?.Count ?? 0) + (expenseData?.Count ?? 0),
                        TotalAmount = (incomeData?.Amount ?? 0) + (expenseData?.Amount ?? 0)
                    };
                })
                .OrderByDescending(u => u.TransactionCount)
                .Take(5)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                NewUsersThisMonth = newUsersThisMonth,
                TotalSystemIncome = totalIncome,
                TotalSystemExpense = totalExpense,
                TotalTransactions = totalTransactions,
                TotalBudgets = totalBudgets,
                RecentUsers = recentUsers,
                TopActiveUsers = topActiveUsers,
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth)
            };

            return View(viewModel);
        }
    }
}