using ExpenseManagementSystem.Constants;
using ExpenseManagementSystem.Data;
using ExpenseManagementSystem.ViewModels.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ExpenseManagementSystem.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class AnalyticsController : Controller
    {
        private readonly ExpenseDbContext _context;

        public AnalyticsController(ExpenseDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var today = DateTime.Today;
            var currentMonth = today.Month;
            var currentYear = today.Year;
            var lastMonth = today.AddMonths(-1);
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            // Current month totals
            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .SumAsync(i => i.Amount);

            var totalExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            // Last month totals
            var lastMonthIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == lastMonth.Month && i.Date.Year == lastMonth.Year)
                .SumAsync(i => i.Amount);

            var lastMonthExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == lastMonth.Month && e.Date.Year == lastMonth.Year)
                .SumAsync(e => e.Amount);

            // Budget analysis
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear)
                .ToListAsync();

            var totalBudget = budgets.Sum(b => b.PlannedAmount);

            var budgetsOnTrack = 0;
            var budgetsOverspent = 0;

            foreach (var budget in budgets)
            {
                var spent = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Category == budget.Category && 
                           e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                if (spent <= budget.PlannedAmount)
                    budgetsOnTrack++;
                else
                    budgetsOverspent++;
            }

            // Daily trends for current month
            var dailyTrends = new List<DailyTrendViewModel>();
            for (int day = 1; day <= Math.Min(today.Day, daysInMonth); day++)
            {
                var date = new DateTime(currentYear, currentMonth, day);
                var dayIncome = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Date == date)
                    .SumAsync(i => i.Amount);
                var dayExpense = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Date == date)
                    .SumAsync(e => e.Amount);

                dailyTrends.Add(new DailyTrendViewModel
                {
                    Day = day,
                    Date = date,
                    Income = dayIncome,
                    Expense = dayExpense
                });
            }

            // Monthly trends (last 6 months)
            var monthlyTrends = new List<MonthlyTrendItemViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                var monthIncome = await _context.Incomes
                    .Where(x => x.UserId == userId && x.Date.Month == monthDate.Month && x.Date.Year == monthDate.Year)
                    .SumAsync(x => x.Amount);
                var monthExpense = await _context.Expenses
                    .Where(x => x.UserId == userId && x.Date.Month == monthDate.Month && x.Date.Year == monthDate.Year)
                    .SumAsync(x => x.Amount);

                monthlyTrends.Add(new MonthlyTrendItemViewModel
                {
                    Month = monthDate.Month,
                    Year = monthDate.Year,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthDate.Month),
                    Income = monthIncome,
                    Expense = monthExpense
                });
            }

            // Category trends (comparing current vs last month)
            var currentMonthCategories = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToListAsync();

            var lastMonthCategories = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == lastMonth.Month && e.Date.Year == lastMonth.Year)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToListAsync();

            var allCategories = currentMonthCategories.Select(c => c.Category)
                .Union(lastMonthCategories.Select(c => c.Category))
                .Distinct()
                .ToList();

            var categoryTrends = allCategories.Select(cat => new CategoryTrendViewModel
            {
                Category = cat,
                CurrentMonth = currentMonthCategories.FirstOrDefault(c => c.Category == cat)?.Amount ?? 0,
                LastMonth = lastMonthCategories.FirstOrDefault(c => c.Category == cat)?.Amount ?? 0
            })
            .OrderByDescending(c => c.CurrentMonth)
            .Take(8)
            .ToList();

            // Generate insights
            var insights = GenerateInsights(totalIncome, totalExpense, lastMonthIncome, lastMonthExpense, 
                totalBudget, budgetsOnTrack, budgetsOverspent, dailyTrends);

            var viewModel = new UserAnalyticsViewModel
            {
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth),
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                LastMonthIncome = lastMonthIncome,
                LastMonthExpense = lastMonthExpense,
                AverageDailyIncome = today.Day > 0 ? totalIncome / today.Day : 0,
                AverageDailyExpense = today.Day > 0 ? totalExpense / today.Day : 0,
                TotalBudget = totalBudget,
                BudgetSpent = totalExpense,
                BudgetsOnTrack = budgetsOnTrack,
                BudgetsOverspent = budgetsOverspent,
                DailyTrends = dailyTrends,
                MonthlyTrends = monthlyTrends,
                CategoryTrends = categoryTrends,
                Insights = insights
            };

            return View(viewModel);
        }

        private List<FinancialInsightViewModel> GenerateInsights(
            decimal totalIncome, decimal totalExpense, 
            decimal lastMonthIncome, decimal lastMonthExpense,
            decimal totalBudget, int budgetsOnTrack, int budgetsOverspent,
            List<DailyTrendViewModel> dailyTrends)
        {
            var insights = new List<FinancialInsightViewModel>();
            var balance = totalIncome - totalExpense;
            var savingsRate = totalIncome > 0 ? ((double)balance / (double)totalIncome) * 100 : 0;

            // Savings insight
            if (savingsRate >= 20)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "success",
                    Icon = "trophy",
                    Title = "Excellent Savings!",
                    Description = $"You're saving {savingsRate:N1}% of your income this month. Keep up the great work!"
                });
            }
            else if (savingsRate >= 10)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "info",
                    Icon = "piggy-bank",
                    Title = "Good Savings Rate",
                    Description = $"You're saving {savingsRate:N1}% of your income. Try to reach 20% for optimal financial health."
                });
            }
            else if (savingsRate >= 0)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "warning",
                    Icon = "exclamation-triangle",
                    Title = "Low Savings",
                    Description = $"Your savings rate is only {savingsRate:N1}%. Consider reducing expenses to improve your savings."
                });
            }
            else
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "danger",
                    Icon = "exclamation-circle",
                    Title = "Overspending Alert",
                    Description = "You're spending more than you earn this month. Review your expenses immediately."
                });
            }

            // Income comparison
            if (lastMonthIncome > 0)
            {
                var incomeChange = ((double)(totalIncome - lastMonthIncome) / (double)lastMonthIncome) * 100;
                if (incomeChange > 10)
                {
                    insights.Add(new FinancialInsightViewModel
                    {
                        Type = "success",
                        Icon = "graph-up-arrow",
                        Title = "Income Increase!",
                        Description = $"Your income increased by {incomeChange:N1}% compared to last month."
                    });
                }
                else if (incomeChange < -10)
                {
                    insights.Add(new FinancialInsightViewModel
                    {
                        Type = "warning",
                        Icon = "graph-down-arrow",
                        Title = "Income Decrease",
                        Description = $"Your income decreased by {Math.Abs(incomeChange):N1}% compared to last month."
                    });
                }
            }

            // Expense comparison
            if (lastMonthExpense > 0)
            {
                var expenseChange = ((double)(totalExpense - lastMonthExpense) / (double)lastMonthExpense) * 100;
                if (expenseChange > 20)
                {
                    insights.Add(new FinancialInsightViewModel
                    {
                        Type = "danger",
                        Icon = "arrow-up-circle",
                        Title = "Expenses Spike",
                        Description = $"Your expenses increased by {expenseChange:N1}% compared to last month. Review your spending."
                    });
                }
                else if (expenseChange < -10)
                {
                    insights.Add(new FinancialInsightViewModel
                    {
                        Type = "success",
                        Icon = "arrow-down-circle",
                        Title = "Expenses Reduced!",
                        Description = $"Great job! You reduced expenses by {Math.Abs(expenseChange):N1}% compared to last month."
                    });
                }
            }

            // Budget insights
            if (budgetsOverspent > 0)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "danger",
                    Icon = "wallet2",
                    Title = "Budget Alert",
                    Description = $"You've exceeded {budgetsOverspent} budget(s) this month. Review your spending in those categories."
                });
            }
            else if (budgetsOnTrack > 0)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "success",
                    Icon = "check-circle",
                    Title = "Budgets On Track",
                    Description = $"All {budgetsOnTrack} of your budgets are within limits. Great financial discipline!"
                });
            }

            // High spending days
            var highSpendingDays = dailyTrends.Where(d => d.Expense > d.Income && d.Expense > 0).Count();
            if (highSpendingDays > dailyTrends.Count * 0.7)
            {
                insights.Add(new FinancialInsightViewModel
                {
                    Type = "warning",
                    Icon = "calendar-x",
                    Title = "Frequent Overspending Days",
                    Description = $"You've had {highSpendingDays} days where expenses exceeded income. Consider spreading purchases."
                });
            }

            return insights;
        }
    }
}
