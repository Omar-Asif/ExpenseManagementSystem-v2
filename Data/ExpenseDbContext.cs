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
        
        // Don't add DbSet for ApplicationUser - we'll configure it differently

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser as a query type that maps to AspNetUsers
            // but don't try to manage it
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.HasKey(e => e.Id);
                // Mark as not tracked by this context
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            // Income configuration
            modelBuilder.Entity<Income>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Category);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Budget configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Month, e.Year, e.Category }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}