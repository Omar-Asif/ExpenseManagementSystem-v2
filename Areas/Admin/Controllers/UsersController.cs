using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _applicationContext;
        private readonly ExpenseDbContext _expenseContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            ApplicationDbContext applicationContext,
            ExpenseDbContext expenseContext,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger)
        {
            _applicationContext = applicationContext;
            _expenseContext = expenseContext;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var users = await GetUsersWithStats(search, status);
            
            var allUsers = await _userManager.GetUsersInRoleAsync(Roles.User);
            
            var viewModel = new UserIndexViewModel
            {
                Users = users,
                TotalCount = allUsers.Count,
                ActiveCount = allUsers.Count(u => u.IsActive),
                InactiveCount = allUsers.Count(u => !u.IsActive),
                SearchTerm = search,
                StatusFilter = status
            };

            return View(viewModel);
        }

        // GET: Admin/Users/Active
        public async Task<IActionResult> Active(string? search)
        {
            return await Index(search, "active");
        }

        // GET: Admin/Users/Inactive
        public async Task<IActionResult> Inactive(string? search)
        {
            return await Index(search, "inactive");
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is in User role
            var isUser = await _userManager.IsInRoleAsync(user, Roles.User);
            if (!isUser)
            {
                return NotFound();
            }

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            // Get financial data
            var totalIncome = await _expenseContext.Incomes
                .Where(i => i.UserId == id)
                .SumAsync(i => i.Amount);

            var totalExpense = await _expenseContext.Expenses
                .Where(e => e.UserId == id)
                .SumAsync(e => e.Amount);

            var incomeCount = await _expenseContext.Incomes
                .Where(i => i.UserId == id)
                .CountAsync();

            var expenseCount = await _expenseContext.Expenses
                .Where(e => e.UserId == id)
                .CountAsync();

            var budgetCount = await _expenseContext.Budgets
                .Where(b => b.UserId == id)
                .CountAsync();

            // Monthly data
            var monthlyIncome = await _expenseContext.Incomes
                .Where(i => i.UserId == id && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var monthlyExpense = await _expenseContext.Expenses
                .Where(e => e.UserId == id && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            // Recent transactions
            var recentIncomes = await _expenseContext.Incomes
                .Where(i => i.UserId == id)
                .OrderByDescending(i => i.Date)
                .Take(5)
                .Select(i => new UserTransactionViewModel
                {
                    Title = i.Title,
                    Amount = i.Amount,
                    Date = i.Date,
                    Type = "Income",
                    Category = null
                })
                .ToListAsync();

            var recentExpenses = await _expenseContext.Expenses
                .Where(e => e.UserId == id)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .Select(e => new UserTransactionViewModel
                {
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

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                IncomeCount = incomeCount,
                ExpenseCount = expenseCount,
                BudgetCount = budgetCount,
                MonthlyIncome = monthlyIncome,
                MonthlyExpense = monthlyExpense,
                RecentTransactions = recentTransactions
            };

            return View(viewModel);
        }

        // GET: Admin/Users/ToggleStatus/5
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isUser = await _userManager.IsInRoleAsync(user, Roles.User);
            if (!isUser)
            {
                return NotFound();
            }

            var viewModel = new ToggleUserStatusViewModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                NewStatus = !user.IsActive
            };

            return View(viewModel);
        }

        // POST: Admin/Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isUser = await _userManager.IsInRoleAsync(user, Roles.User);
            if (!isUser)
            {
                return NotFound();
            }

            var previousStatus = user.IsActive;
            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var action = user.IsActive ? "activated" : "deactivated";
                _logger.LogInformation("User {Email} has been {Action} by admin", user.Email, action);
                TempData["SuccessMessage"] = $"User {user.FirstName} {user.LastName} has been {action} successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update user status.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<UserListViewModel>> GetUsersWithStats(string? search, string? status)
        {
            var allUsers = await _userManager.GetUsersInRoleAsync(Roles.User);
            var usersList = allUsers.ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                usersList = usersList.Where(u =>
                    (u.FirstName?.ToLower().Contains(search) ?? false) ||
                    (u.LastName?.ToLower().Contains(search) ?? false) ||
                    (u.Email?.ToLower().Contains(search) ?? false)
                ).ToList();
            }

            // Apply status filter
            if (status == "active")
            {
                usersList = usersList.Where(u => u.IsActive).ToList();
            }
            else if (status == "inactive")
            {
                usersList = usersList.Where(u => !u.IsActive).ToList();
            }

            var userIds = usersList.Select(u => u.Id).ToList();

            // Get income stats
            var incomeByUser = await _expenseContext.Incomes
                .Where(i => userIds.Contains(i.UserId))
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(i => i.Amount), Count = g.Count() })
                .ToListAsync();

            // Get expense stats
            var expenseByUser = await _expenseContext.Expenses
                .Where(e => userIds.Contains(e.UserId))
                .GroupBy(e => e.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                .ToListAsync();

            return usersList.Select(u =>
            {
                var income = incomeByUser.FirstOrDefault(i => i.UserId == u.Id);
                var expense = expenseByUser.FirstOrDefault(e => e.UserId == u.Id);
                return new UserListViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email ?? string.Empty,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    TransactionCount = (income?.Count ?? 0) + (expense?.Count ?? 0),
                    TotalIncome = income?.Total ?? 0,
                    TotalExpense = expense?.Total ?? 0
                };
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToList();
        }
    }
}
