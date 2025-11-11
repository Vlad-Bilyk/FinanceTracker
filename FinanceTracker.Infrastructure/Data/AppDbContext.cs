using FinanceTracker.Application.Common;
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
             .HasMaxLength(ValidationConstants.OperationTypeNameMaxLength)
             .IsRequired();

            e.Property(x => x.Description)
             .HasMaxLength(ValidationConstants.OperationTypeDescriptionMaxLength);

            e.Property(x => x.Kind)
             .HasConversion<string>();

            e.HasIndex(x => new { x.Kind, x.Name })
             .IsUnique();
        });

        modelBuilder.Entity<FinancialOperation>(e =>
        {
            e.Property(x => x.Note)
             .HasMaxLength(ValidationConstants.OperationNoteMaxLength);

            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Type)
             .WithMany(t => t.Operations)
             .HasForeignKey(x => x.TypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
