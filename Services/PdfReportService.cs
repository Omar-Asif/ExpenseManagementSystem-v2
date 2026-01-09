using ExpenseManagementSystem.ViewModels.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExpenseManagementSystem.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateMonthlyReport(MonthlyReportDetailViewModel report);
        byte[] GenerateYearlyReport(YearlyReportDetailViewModel report);
    }

    public class PdfReportService : IPdfReportService
    {
        public PdfReportService()
        {
            // Set QuestPDF license type (Community is free for revenue < $1M)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateMonthlyReport(MonthlyReportDetailViewModel report)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(container => ComposeHeader(container, 
                        $"Monthly Financial Report", 
                        $"{report.MonthName} {report.Year}",
                        report.UserName));

                    page.Content().Element(container => ComposeMonthlyContent(container, report));

                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateYearlyReport(YearlyReportDetailViewModel report)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(container => ComposeHeader(container, 
                        $"Yearly Financial Report", 
                        $"Year {report.Year}",
                        report.UserName));

                    page.Content().Element(container => ComposeYearlyContent(container, report));

                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, string title, string period, string userName)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(title)
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Indigo.Darken2);

                        col.Item().Text(period)
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().AlignRight().Text("ExpenseManager")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Indigo.Medium);

                        col.Item().AlignRight().Text(userName)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        col.Item().AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeMonthlyContent(IContainer container, MonthlyReportDetailViewModel report)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Financial Summary
                column.Item().Element(c => ComposeSummarySection(c, 
                    report.TotalIncome, 
                    report.TotalExpense, 
                    report.Balance,
                    report.SavingsRate));

                // Budget Summary (if any)
                if (report.Budgets.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeBudgetSection(c, report.Budgets, report.TotalBudget, report.BudgetUsed));
                }

                // Expense by Category
                if (report.ExpenseByCategory.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeCategorySection(c, report.ExpenseByCategory));
                }

                // Income Transactions
                if (report.Incomes.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeIncomeTable(c, report.Incomes));
                }

                // Expense Transactions
                if (report.Expenses.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeExpenseTable(c, report.Expenses));
                }
            });
        }

        private void ComposeYearlyContent(IContainer container, YearlyReportDetailViewModel report)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Financial Summary
                column.Item().Element(c => ComposeSummarySection(c, 
                    report.TotalIncome, 
                    report.TotalExpense, 
                    report.Balance,
                    report.SavingsRate));

                // Monthly Breakdown
                if (report.MonthlyBreakdown.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeMonthlyBreakdown(c, report.MonthlyBreakdown));
                }

                // Expense by Category
                if (report.ExpenseByCategory.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeCategorySection(c, report.ExpenseByCategory));
                }

                // Top Income Sources
                if (report.TopIncomeSources.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeTopItemsSection(c, "Top Income Sources", report.TopIncomeSources, Colors.Green.Medium));
                }

                // Top Expense Categories
                if (report.TopExpenseCategories.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeTopItemsSection(c, "Top Expense Categories", report.TopExpenseCategories, Colors.Red.Medium));
                }
            });
        }

        private void ComposeSummarySection(IContainer container, decimal income, decimal expense, decimal balance, double savingsRate)
        {
            container.Column(column =>
            {
                column.Item().Text("Financial Summary")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeSummaryBox(c, "Total Income", income, Colors.Green.Medium));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeSummaryBox(c, "Total Expenses", expense, Colors.Red.Medium));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeSummaryBox(c, "Balance", balance, balance >= 0 ? Colors.Blue.Medium : Colors.Red.Medium));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeSummaryBox(c, "Savings Rate", savingsRate, savingsRate >= 0 ? Colors.Green.Medium : Colors.Red.Medium, true));
                });
            });
        }

        private void ComposeSummaryBox(IContainer container, string label, decimal value, string color, bool isPercentage = false)
        {
            container.Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text(label)
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                    
                    column.Item().Text(isPercentage ? $"{value:N1}%" : $"${value:N2}")
                        .FontSize(14)
                        .Bold()
                        .FontColor(color);
                });
        }

        private void ComposeSummaryBox(IContainer container, string label, double value, string color, bool isPercentage = false)
        {
            ComposeSummaryBox(container, label, (decimal)value, color, isPercentage);
        }

        private void ComposeBudgetSection(IContainer container, List<ReportBudgetViewModel> budgets, decimal totalBudget, decimal budgetUsed)
        {
            container.Column(column =>
            {
                column.Item().Text("Budget Overview")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).Text("Category").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Planned").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Spent").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Remaining").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Used").Bold();
                    });

                    foreach (var budget in budgets)
                    {
                        var rowColor = budget.IsOverBudget ? Colors.Red.Lighten5 : Colors.White;
                        
                        table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(budget.Category);
                        table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${budget.PlannedAmount:N2}");
                        table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${budget.SpentAmount:N2}").FontColor(Colors.Red.Medium);
                        table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${budget.RemainingAmount:N2}").FontColor(budget.RemainingAmount >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{budget.UsedPercentage:N0}%");
                    }
                });
            });
        }

        private void ComposeCategorySection(IContainer container, List<CategorySummaryViewModel> categories)
        {
            container.Column(column =>
            {
                column.Item().Text("Expenses by Category")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("Category").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Amount").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Count").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Share").Bold();
                    });

                    foreach (var category in categories.OrderByDescending(c => c.Amount))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(category.Category);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${category.Amount:N2}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(category.Count.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{category.Percentage:N1}%");
                    }
                });
            });
        }

        private void ComposeIncomeTable(IContainer container, List<ReportIncomeViewModel> incomes)
        {
            container.Column(column =>
            {
                column.Item().Text("Income Transactions")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("Date").Bold();
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("Title").Bold();
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("Description").Bold();
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5).AlignRight().Text("Amount").Bold();
                    });

                    foreach (var income in incomes.OrderByDescending(i => i.Date))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(income.Date.ToString("MMM dd"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(income.Title);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(income.Description ?? "-").FontColor(Colors.Grey.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${income.Amount:N2}").FontColor(Colors.Green.Medium);
                    }

                    // Total row
                    table.Cell().ColumnSpan(3).Background(Colors.Green.Lighten5).Padding(5).AlignRight().Text("Total Income:").Bold();
                    table.Cell().Background(Colors.Green.Lighten5).Padding(5).AlignRight().Text($"${incomes.Sum(i => i.Amount):N2}").Bold().FontColor(Colors.Green.Medium);
                });
            });
        }

        private void ComposeExpenseTable(IContainer container, List<ReportExpenseViewModel> expenses)
        {
            container.Column(column =>
            {
                column.Item().Text("Expense Transactions")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("Date").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("Title").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("Category").Bold();
                        header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Amount").Bold();
                    });

                    foreach (var expense in expenses.OrderByDescending(e => e.Date))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(expense.Date.ToString("MMM dd"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(expense.Title);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(expense.Category);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${expense.Amount:N2}").FontColor(Colors.Red.Medium);
                    }

                    // Total row
                    table.Cell().ColumnSpan(3).Background(Colors.Red.Lighten5).Padding(5).AlignRight().Text("Total Expenses:").Bold();
                    table.Cell().Background(Colors.Red.Lighten5).Padding(5).AlignRight().Text($"${expenses.Sum(e => e.Amount):N2}").Bold().FontColor(Colors.Red.Medium);
                });
            });
        }

        private void ComposeMonthlyBreakdown(IContainer container, List<MonthlyReportViewModel> months)
        {
            container.Column(column =>
            {
                column.Item().Text("Monthly Breakdown")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).Text("Month").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Income").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Expenses").Bold();
                        header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Balance").Bold();
                    });

                    foreach (var month in months)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(month.MonthName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${month.Income:N2}").FontColor(Colors.Green.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${month.Expense:N2}").FontColor(Colors.Red.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${month.Balance:N2}").FontColor(month.Balance >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                    }

                    // Total row
                    table.Cell().Background(Colors.Indigo.Lighten5).Padding(5).Text("Yearly Total").Bold();
                    table.Cell().Background(Colors.Indigo.Lighten5).Padding(5).AlignRight().Text($"${months.Sum(m => m.Income):N2}").Bold().FontColor(Colors.Green.Medium);
                    table.Cell().Background(Colors.Indigo.Lighten5).Padding(5).AlignRight().Text($"${months.Sum(m => m.Expense):N2}").Bold().FontColor(Colors.Red.Medium);
                    table.Cell().Background(Colors.Indigo.Lighten5).Padding(5).AlignRight().Text($"${months.Sum(m => m.Balance):N2}").Bold().FontColor(months.Sum(m => m.Balance) >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                });
            });
        }

        private void ComposeTopItemsSection(IContainer container, string title, List<TopItemViewModel> items, string color)
        {
            container.Column(column =>
            {
                column.Item().Text(title)
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    var rank = 1;
                    foreach (var item in items.Take(5))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"#{rank}").FontColor(Colors.Grey.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Name);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${item.Amount:N2}").FontColor(color);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Count}x");
                        rank++;
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text("Expense Management System")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);

                    row.RelativeItem().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Text("Developed by S.M. Shah Omar Asif")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
