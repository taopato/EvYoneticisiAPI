using Microsoft.EntityFrameworkCore;
using EvYoneticisiAPI.Models;

namespace EvYoneticisiAPI.Data
{
    public class EvYoneticisiContext : DbContext
    {
        public EvYoneticisiContext(DbContextOptions<EvYoneticisiContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Debt> Debts { get; set; }
        public DbSet<UserExpense> UserExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.Debtor)
                .WithMany()
                .HasForeignKey(d => d.DebtorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.Creditor)
                .WithMany()
                .HasForeignKey(d => d.CreditorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserExpense>()
                .HasOne(ue => ue.Expense)
                .WithMany(e => e.UserExpenses)
                .HasForeignKey(ue => ue.ExpenseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserExpense>()
                .HasOne(ue => ue.User)
                .WithMany(u => u.UserExpenses)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
