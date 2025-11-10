using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<FinancialOperationType> FinancialOperationTypes { get; set; }
    public DbSet<FinancialOperation> FinancialOperations { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FinancialOperationType>(e =>
        {
            e.Property(x => x.Name)
             .HasMaxLength(100)
             .IsRequired();

            e.Property(x => x.Description)
             .HasMaxLength(500);

            e.Property(x => x.Kind)
             .HasConversion<string>();

            e.HasIndex(x => new { x.Kind, x.Name })
             .IsUnique();
        });

        modelBuilder.Entity<FinancialOperation>(e =>
        {
            e.Property(x => x.Note)
             .HasMaxLength(500);

            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Type)
             .WithMany(t => t.Operations)
             .HasForeignKey(x => x.TypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
