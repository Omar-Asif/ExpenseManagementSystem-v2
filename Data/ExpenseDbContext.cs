using ExpenseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagementSystem.Data
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
            : base(options)
        {
        }

        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Income configuration
            modelBuilder.Entity<Income>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Category);
            });

            // Budget configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Month, e.Year, e.Category }).IsUnique();
            });
        }
    }
}