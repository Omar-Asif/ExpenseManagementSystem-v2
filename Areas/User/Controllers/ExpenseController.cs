using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.ViewModels.Expense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class ExpenseController : Controller
    {
        private readonly ExpenseDbContext _context;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(ExpenseDbContext context, ILogger<ExpenseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET: User/Expense
        public async Task<IActionResult> Index(int? month, int? year, string? category)
        {
            var userId = GetUserId();
            var currentMonth = month ?? DateTime.Today.Month;
            var currentYear = year ?? DateTime.Today.Year;

            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (month.HasValue || year.HasValue)
            {
                query = query.Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category == category);
            }

            var expenses = await query
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .Select(e => new ExpenseListViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Date = e.Date,
                    Category = e.Category,
                    Description = e.Description
                })
                .ToListAsync();

            // Get all categories used by this user
            var usedCategories = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => e.Category)
                .Distinct()
                .ToListAsync();

            var viewModel = new ExpenseIndexViewModel
            {
                Expenses = expenses,
                TotalExpense = expenses.Sum(e => e.Amount),
                CurrentMonth = DateTime.Today.Month,
                CurrentYear = DateTime.Today.Year,
                FilterMonth = month,
                FilterYear = year,
                FilterCategory = category,
                Categories = usedCategories
            };

            return View(viewModel);
        }

        // GET: User/Expense/Create
        public IActionResult Create()
        {
            ViewBag.Categories = ExpenseCategories.Categories;
            var viewModel = new ExpenseCreateViewModel
            {
                Date = DateTime.Today
            };
            return View(viewModel);
        }

        // POST: User/Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = ExpenseCategories.Categories;
                return View(viewModel);
            }

            // Server-side validation for future date
            if (viewModel.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future");
                ViewBag.Categories = ExpenseCategories.Categories;
                return View(viewModel);
            }

            var expense = new Expense
            {
                Title = viewModel.Title,
                Amount = viewModel.Amount,
                Date = viewModel.Date,
                Category = viewModel.Category,
                Description = viewModel.Description,
                UserId = GetUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense created: {Title} - {Amount} - {Category} by user {UserId}",
                expense.Title, expense.Amount, expense.Category, expense.UserId);

            TempData["SuccessMessage"] = "Expense added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Expense/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = ExpenseCategories.Categories;
            var viewModel = new ExpenseEditViewModel
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description
            };

            return View(viewModel);
        }

        // POST: User/Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = ExpenseCategories.Categories;
                return View(viewModel);
            }

            // Server-side validation for future date
            if (viewModel.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future");
                ViewBag.Categories = ExpenseCategories.Categories;
                return View(viewModel);
            }

            var userId = GetUserId();
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            expense.Title = viewModel.Title;
            expense.Amount = viewModel.Amount;
            expense.Date = viewModel.Date;
            expense.Category = viewModel.Category;
            expense.Description = viewModel.Description;
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense updated: {Id} - {Title} by user {UserId}",
                expense.Id, expense.Title, expense.UserId);

            TempData["SuccessMessage"] = "Expense updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Expense/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            var viewModel = new ExpenseListViewModel
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description
            };

            return View(viewModel);
        }

        // POST: User/Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense deleted: {Id} - {Title} by user {UserId}",
                expense.Id, expense.Title, expense.UserId);

            TempData["SuccessMessage"] = "Expense deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
