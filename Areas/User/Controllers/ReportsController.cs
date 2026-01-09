using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.Models;
using ExpenseManagementSystem.Services;
using ExpenseManagementSystem.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class ReportsController : Controller
    {
        private readonly ExpenseDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPdfReportService _pdfService;

        public ReportsController(
            ExpenseDbContext context,
            UserManager<ApplicationUser> userManager,
            IPdfReportService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET: User/Reports
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            // Monthly data
            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var monthlyExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            var monthlyIncomeCount = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .CountAsync();

            var monthlyExpenseCount = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .CountAsync();

            // Yearly data
            var yearlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var yearlyExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            var yearlyIncomeCount = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Year == currentYear)
                .CountAsync();

            var yearlyExpenseCount = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Year == currentYear)
                .CountAsync();

            // Category breakdown for current month
            var categoryBreakdown = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummaryViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToListAsync();

            if (monthlyExpense > 0)
            {
                foreach (var cat in categoryBreakdown)
                {
                    cat.Percentage = (double)cat.Amount / (double)monthlyExpense * 100;
                }
            }

            // Monthly trends for the year
            var monthlyTrends = new List<MonthlyReportViewModel>();
            for (int month = 1; month <= 12; month++)
            {
                var income = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == month && i.Date.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var expense = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == month && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                var incomeCount = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == month && i.Date.Year == currentYear)
                    .CountAsync();

                var expenseCount = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == month && e.Date.Year == currentYear)
                    .CountAsync();

                monthlyTrends.Add(new MonthlyReportViewModel
                {
                    Month = month,
                    Year = currentYear,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    Income = income,
                    Expense = expense,
                    IncomeCount = incomeCount,
                    ExpenseCount = expenseCount
                });
            }

            // Available years
            var incomeYears = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Select(i => i.Date.Year)
                .Distinct()
                .ToListAsync();

            var expenseYears = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => e.Date.Year)
                .Distinct()
                .ToListAsync();

            var availableYears = incomeYears.Union(expenseYears)
                .Union(new[] { currentYear })
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            var viewModel = new ReportIndexViewModel
            {
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth),
                MonthlyIncome = monthlyIncome,
                MonthlyExpense = monthlyExpense,
                MonthlyIncomeCount = monthlyIncomeCount,
                MonthlyExpenseCount = monthlyExpenseCount,
                YearlyIncome = yearlyIncome,
                YearlyExpense = yearlyExpense,
                YearlyIncomeCount = yearlyIncomeCount,
                YearlyExpenseCount = yearlyExpenseCount,
                ExpenseByCategory = categoryBreakdown,
                MonthlyTrends = monthlyTrends,
                AvailableYears = availableYears
            };

            return View(viewModel);
        }

        // GET: User/Reports/DownloadMonthly
        public async Task<IActionResult> DownloadMonthly(int? month, int? year)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var targetMonth = month ?? DateTime.Today.Month;
            var targetYear = year ?? DateTime.Today.Year;

            var report = await GetMonthlyReportData(userId, targetMonth, targetYear);
            report.UserName = user != null ? $"{user.FirstName} {user.LastName}" : "User";

            var pdfBytes = _pdfService.GenerateMonthlyReport(report);

            return File(pdfBytes, "application/pdf", 
                $"MonthlyReport_{report.MonthName}_{report.Year}.pdf");
        }

        // GET: User/Reports/DownloadYearly
        public async Task<IActionResult> DownloadYearly(int? year)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var targetYear = year ?? DateTime.Today.Year;

            var report = await GetYearlyReportData(userId, targetYear);
            report.UserName = user != null ? $"{user.FirstName} {user.LastName}" : "User";

            var pdfBytes = _pdfService.GenerateYearlyReport(report);

            return File(pdfBytes, "application/pdf", 
                $"YearlyReport_{report.Year}.pdf");
        }

        // GET: User/Reports/Monthly
        public async Task<IActionResult> Monthly(int? month, int? year)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var targetMonth = month ?? DateTime.Today.Month;
            var targetYear = year ?? DateTime.Today.Year;

            var report = await GetMonthlyReportData(userId, targetMonth, targetYear);
            report.UserName = user != null ? $"{user.FirstName} {user.LastName}" : "User";

            ViewBag.AvailableMonths = Enumerable.Range(1, 12)
                .Select(m => new { Value = m, Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                .ToList();

            ViewBag.AvailableYears = await GetAvailableYears(userId);

            return View(report);
        }

        // GET: User/Reports/Yearly
        public async Task<IActionResult> Yearly(int? year)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var targetYear = year ?? DateTime.Today.Year;

            var report = await GetYearlyReportData(userId, targetYear);
            report.UserName = user != null ? $"{user.FirstName} {user.LastName}" : "User";

            ViewBag.AvailableYears = await GetAvailableYears(userId);

            return View(report);
        }

        private async Task<MonthlyReportDetailViewModel> GetMonthlyReportData(string userId, int month, int year)
        {
            // Get incomes
            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == month && i.Date.Year == year)
                .OrderByDescending(i => i.Date)
                .Select(i => new ReportIncomeViewModel
                {
                    Title = i.Title,
                    Amount = i.Amount,
                    Date = i.Date,
                    Description = i.Description
                })
                .ToListAsync();

            // Get expenses
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == month && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .Select(e => new ReportExpenseViewModel
                {
                    Title = e.Title,
                    Amount = e.Amount,
                    Date = e.Date,
                    Category = e.Category,
                    Description = e.Description
                })
                .ToListAsync();

            // Get budgets
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
                .ToListAsync();

            var expenseByCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummaryViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .ToList();

            var totalExpense = expenses.Sum(e => e.Amount);
            foreach (var cat in expenseByCategory)
            {
                cat.Percentage = totalExpense > 0 ? (double)cat.Amount / (double)totalExpense * 100 : 0;
            }

            var budgetViewModels = budgets.Select(b =>
            {
                var spent = expenseByCategory.FirstOrDefault(c => c.Category == b.Category)?.Amount ?? 0;
                return new ReportBudgetViewModel
                {
                    Category = b.Category,
                    PlannedAmount = b.PlannedAmount,
                    SpentAmount = spent
                };
            }).ToList();

            return new MonthlyReportDetailViewModel
            {
                Month = month,
                Year = year,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalIncome = incomes.Sum(i => i.Amount),
                TotalExpense = totalExpense,
                TotalBudget = budgets.Sum(b => b.PlannedAmount),
                BudgetUsed = totalExpense,
                Incomes = incomes,
                Expenses = expenses,
                ExpenseByCategory = expenseByCategory.OrderByDescending(c => c.Amount).ToList(),
                Budgets = budgetViewModels
            };
        }

        private async Task<YearlyReportDetailViewModel> GetYearlyReportData(string userId, int year)
        {
            // Get all incomes for the year
            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Year == year)
                .ToListAsync();

            // Get all expenses for the year
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Year == year)
                .ToListAsync();

            // Monthly breakdown
            var monthlyBreakdown = new List<MonthlyReportViewModel>();
            for (int month = 1; month <= 12; month++)
            {
                var monthIncome = incomes.Where(i => i.Date.Month == month).Sum(i => i.Amount);
                var monthExpense = expenses.Where(e => e.Date.Month == month).Sum(e => e.Amount);
                var monthIncomeCount = incomes.Count(i => i.Date.Month == month);
                var monthExpenseCount = expenses.Count(e => e.Date.Month == month);

                monthlyBreakdown.Add(new MonthlyReportViewModel
                {
                    Month = month,
                    Year = year,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    Income = monthIncome,
                    Expense = monthExpense,
                    IncomeCount = monthIncomeCount,
                    ExpenseCount = monthExpenseCount
                });
            }

            // Expense by category
            var totalExpense = expenses.Sum(e => e.Amount);
            var expenseByCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummaryViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count(),
                    Percentage = totalExpense > 0 ? (double)g.Sum(e => e.Amount) / (double)totalExpense * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            // Top income sources
            var topIncomeSources = incomes
                .GroupBy(i => i.Title)
                .Select(g => new TopItemViewModel
                {
                    Name = g.Key,
                    Amount = g.Sum(i => i.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Amount)
                .Take(5)
                .ToList();

            // Top expense categories
            var topExpenseCategories = expenses
                .GroupBy(e => e.Category)
                .Select(g => new TopItemViewModel
                {
                    Name = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Amount)
                .Take(5)
                .ToList();

            return new YearlyReportDetailViewModel
            {
                Year = year,
                TotalIncome = incomes.Sum(i => i.Amount),
                TotalExpense = totalExpense,
                IncomeCount = incomes.Count,
                ExpenseCount = expenses.Count,
                MonthlyBreakdown = monthlyBreakdown,
                ExpenseByCategory = expenseByCategory,
                TopIncomeSources = topIncomeSources,
                TopExpenseCategories = topExpenseCategories
            };
        }

        private async Task<List<int>> GetAvailableYears(string userId)
        {
            var currentYear = DateTime.Today.Year;

            var incomeYears = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Select(i => i.Date.Year)
                .Distinct()
                .ToListAsync();

            var expenseYears = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => e.Date.Year)
                .Distinct()
                .ToListAsync();

            return incomeYears.Union(expenseYears)
                .Union(new[] { currentYear })
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
        }
    }
}
