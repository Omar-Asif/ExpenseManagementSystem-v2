using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.ViewModels.Budget;
using ExpenseManagementSystem.ViewModels.Expense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class BudgetController : Controller
    {
        private readonly ExpenseDbContext _context;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(ExpenseDbContext context, ILogger<BudgetController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET: User/Budget
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var userId = GetUserId();
            var filterMonth = month ?? DateTime.Today.Month;
            var filterYear = year ?? DateTime.Today.Year;

            // Get budgets for the selected month/year
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == filterMonth && b.Year == filterYear)
                .OrderBy(b => b.Category)
                .ToListAsync();

            // Get expenses for the same period to calculate spent amounts
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == filterMonth && e.Date.Year == filterYear)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .ToListAsync();

            var budgetList = budgets.Select(b => new BudgetListViewModel
            {
                Id = b.Id,
                Category = b.Category,
                PlannedAmount = b.PlannedAmount,
                SpentAmount = expenses.FirstOrDefault(e => e.Category == b.Category)?.Total ?? 0,
                Month = b.Month,
                Year = b.Year
            }).ToList();

            var viewModel = new BudgetIndexViewModel
            {
                Budgets = budgetList,
                TotalPlanned = budgetList.Sum(b => b.PlannedAmount),
                TotalSpent = budgetList.Sum(b => b.SpentAmount),
                CurrentMonth = DateTime.Today.Month,
                CurrentYear = DateTime.Today.Year,
                FilterMonth = filterMonth,
                FilterYear = filterYear
            };

            return View(viewModel);
        }

        // GET: User/Budget/Create
        public IActionResult Create()
        {
            ViewBag.Categories = ExpenseCategories.Categories;
            ViewBag.Months = GetMonthsList();
            
            var viewModel = new BudgetCreateViewModel
            {
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year
            };
            return View(viewModel);
        }

        // POST: User/Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = ExpenseCategories.Categories;
                ViewBag.Months = GetMonthsList();
                return View(viewModel);
            }

            var userId = GetUserId();

            // Check if budget already exists for this category/month/year
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId 
                    && b.Category == viewModel.Category 
                    && b.Month == viewModel.Month 
                    && b.Year == viewModel.Year);

            if (existingBudget != null)
            {
                ModelState.AddModelError("Category", "A budget for this category already exists for the selected month/year");
                ViewBag.Categories = ExpenseCategories.Categories;
                ViewBag.Months = GetMonthsList();
                return View(viewModel);
            }

            // Validate not future month
            var budgetDate = new DateTime(viewModel.Year, viewModel.Month, 1);
            var currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (budgetDate > currentDate)
            {
                ModelState.AddModelError("Month", "Cannot create budget for future months");
                ViewBag.Categories = ExpenseCategories.Categories;
                ViewBag.Months = GetMonthsList();
                return View(viewModel);
            }

            var budget = new Budget
            {
                Category = viewModel.Category,
                PlannedAmount = viewModel.PlannedAmount,
                Month = viewModel.Month,
                Year = viewModel.Year,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Budget created: {Category} - {Amount} for {Month}/{Year} by user {UserId}",
                budget.Category, budget.PlannedAmount, budget.Month, budget.Year, budget.UserId);

            TempData["SuccessMessage"] = "Budget created successfully!";
            return RedirectToAction(nameof(Index), new { month = viewModel.Month, year = viewModel.Year });
        }

        // GET: User/Budget/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            ViewBag.Categories = ExpenseCategories.Categories;
            ViewBag.Months = GetMonthsList();
            
            var viewModel = new BudgetEditViewModel
            {
                Id = budget.Id,
                Category = budget.Category,
                PlannedAmount = budget.PlannedAmount,
                Month = budget.Month,
                Year = budget.Year
            };

            return View(viewModel);
        }

        // POST: User/Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = ExpenseCategories.Categories;
                ViewBag.Months = GetMonthsList();
                return View(viewModel);
            }

            var userId = GetUserId();
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            // Check if another budget exists for this category/month/year (excluding current)
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId 
                    && b.Category == viewModel.Category 
                    && b.Month == viewModel.Month 
                    && b.Year == viewModel.Year
                    && b.Id != id);

            if (existingBudget != null)
            {
                ModelState.AddModelError("Category", "A budget for this category already exists for the selected month/year");
                ViewBag.Categories = ExpenseCategories.Categories;
                ViewBag.Months = GetMonthsList();
                return View(viewModel);
            }

            budget.Category = viewModel.Category;
            budget.PlannedAmount = viewModel.PlannedAmount;
            budget.Month = viewModel.Month;
            budget.Year = viewModel.Year;
            budget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Budget updated: {Id} - {Category} by user {UserId}",
                budget.Id, budget.Category, budget.UserId);

            TempData["SuccessMessage"] = "Budget updated successfully!";
            return RedirectToAction(nameof(Index), new { month = viewModel.Month, year = viewModel.Year });
        }

        // GET: User/Budget/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            // Get spent amount for this budget
            var spentAmount = await _context.Expenses
                .Where(e => e.UserId == userId 
                    && e.Category == budget.Category 
                    && e.Date.Month == budget.Month 
                    && e.Date.Year == budget.Year)
                .SumAsync(e => e.Amount);

            var viewModel = new BudgetListViewModel
            {
                Id = budget.Id,
                Category = budget.Category,
                PlannedAmount = budget.PlannedAmount,
                SpentAmount = spentAmount,
                Month = budget.Month,
                Year = budget.Year
            };

            return View(viewModel);
        }

        // POST: User/Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            var month = budget.Month;
            var year = budget.Year;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Budget deleted: {Id} - {Category} by user {UserId}",
                budget.Id, budget.Category, budget.UserId);

            TempData["SuccessMessage"] = "Budget deleted successfully!";
            return RedirectToAction(nameof(Index), new { month, year });
        }

        private List<(int Value, string Name)> GetMonthsList()
        {
            return Enumerable.Range(1, 12)
                .Select(i => (i, new DateTime(2000, i, 1).ToString("MMMM")))
                .ToList();
        }
    }
}
