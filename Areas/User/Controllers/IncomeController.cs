using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.ViewModels.Income;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class IncomeController : Controller
    {
        private readonly ExpenseDbContext _context;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(ExpenseDbContext context, ILogger<IncomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET: User/Income
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var userId = GetUserId();
            var currentMonth = month ?? DateTime.Today.Month;
            var currentYear = year ?? DateTime.Today.Year;

            var query = _context.Incomes
                .Where(i => i.UserId == userId)
                .AsQueryable();

            if (month.HasValue || year.HasValue)
            {
                query = query.Where(i => i.Date.Month == currentMonth && i.Date.Year == currentYear);
            }

            var incomes = await query
                .OrderByDescending(i => i.Date)
                .ThenByDescending(i => i.CreatedAt)
                .Select(i => new IncomeListViewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Amount = i.Amount,
                    Date = i.Date,
                    Description = i.Description
                })
                .ToListAsync();

            var viewModel = new IncomeIndexViewModel
            {
                Incomes = incomes,
                TotalIncome = incomes.Sum(i => i.Amount),
                CurrentMonth = DateTime.Today.Month,
                CurrentYear = DateTime.Today.Year,
                FilterMonth = month,
                FilterYear = year
            };

            return View(viewModel);
        }

        // GET: User/Income/Create
        public IActionResult Create()
        {
            var viewModel = new IncomeCreateViewModel
            {
                Date = DateTime.Today
            };
            return View(viewModel);
        }

        // POST: User/Income/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IncomeCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Server-side validation for future date
            if (viewModel.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future");
                return View(viewModel);
            }

            var income = new Income
            {
                Title = viewModel.Title,
                Amount = viewModel.Amount,
                Date = viewModel.Date,
                Description = viewModel.Description,
                UserId = GetUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Incomes.Add(income);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Income created: {Title} - {Amount} by user {UserId}", 
                income.Title, income.Amount, income.UserId);

            TempData["SuccessMessage"] = "Income added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Income/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            var viewModel = new IncomeEditViewModel
            {
                Id = income.Id,
                Title = income.Title,
                Amount = income.Amount,
                Date = income.Date,
                Description = income.Description
            };

            return View(viewModel);
        }

        // POST: User/Income/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncomeEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Server-side validation for future date
            if (viewModel.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future");
                return View(viewModel);
            }

            var userId = GetUserId();
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            income.Title = viewModel.Title;
            income.Amount = viewModel.Amount;
            income.Date = viewModel.Date;
            income.Description = viewModel.Description;
            income.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Income updated: {Id} - {Title} by user {UserId}", 
                income.Id, income.Title, income.UserId);

            TempData["SuccessMessage"] = "Income updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Income/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            var viewModel = new IncomeListViewModel
            {
                Id = income.Id,
                Title = income.Title,
                Amount = income.Amount,
                Date = income.Date,
                Description = income.Description
            };

            return View(viewModel);
        }

        // POST: User/Income/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Income deleted: {Id} - {Title} by user {UserId}", 
                income.Id, income.Title, income.UserId);

            TempData["SuccessMessage"] = "Income deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
