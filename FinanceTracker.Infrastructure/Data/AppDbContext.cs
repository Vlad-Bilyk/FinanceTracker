using FinanceTracker.Application.Common;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<FinancialOperationType> FinancialOperationTypes { get; set; }
    public DbSet<FinancialOperation> FinancialOperations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Currency> Currencies { get; set; }

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

            e.HasIndex(x => new { x.UserId, x.Kind, x.Name })
             .IsUnique();
        });

        modelBuilder.Entity<FinancialOperation>(e =>
        {
            e.Property(x => x.TypeId).IsRequired();
            e.Property(x => x.WalletId).IsRequired();

            e.Property(x => x.AmountBase)
             .HasPrecision(18, 2);

            e.Property(x => x.AmountOriginal)
            .HasPrecision(18, 2);

            e.Property(x => x.Date)
             .HasColumnType("timestamp")
             .IsRequired();

            e.Property(x => x.Note)
             .HasMaxLength(ValidationConstants.OperationNoteMaxLength);

            e.Property(x => x.CurrencyOriginalCode)
             .HasMaxLength(ValidationConstants.CurrencyCodeLength)
             .IsFixedLength();

            e.HasOne(x => x.Type)
             .WithMany(t => t.Operations)
             .HasForeignKey(x => x.TypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Currency)
             .WithMany()
             .HasForeignKey(x => x.CurrencyOriginalCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.UserName)
             .HasMaxLength(ValidationConstants.UserNameMaxLength)
             .IsRequired();

            e.Property(x => x.PasswordHash)
             .IsRequired();

            e.HasIndex(x => x.UserName)
             .IsUnique();

            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Wallet>(e =>
        {
            e.Property(x => x.Name)
             .HasMaxLength(ValidationConstants.WalletNameMaxLength)
             .IsRequired();

            e.Property(w => w.BaseCurrencyCode)
             .HasMaxLength(3)
             .IsFixedLength()
             .IsRequired();

            e.HasIndex(x => new { x.UserId, x.Name })
             .IsUnique();

            e.HasOne(x => x.User)
             .WithMany(u => u.Wallets)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Operations)
             .WithOne(o => o.Wallet)
             .HasForeignKey(o => o.WalletId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.BaseCurrency)
             .WithMany()
             .HasForeignKey(x => x.BaseCurrencyCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Currency>(e =>
        {
            e.HasKey(x => x.Code);

            e.Property(x => x.Code)
             .HasMaxLength(ValidationConstants.CurrencyCodeLength)
             .IsFixedLength()
             .IsRequired();

            e.Property(x => x.Name)
             .HasMaxLength(ValidationConstants.CurrencyNameMaxLength)
             .IsRequired();
        });
    }
}
