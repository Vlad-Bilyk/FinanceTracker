using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data.Seed;

/// <summary>
/// Seeding a database with initial data
/// </summary>
public class DbSeeder : IDbSeeder
{
    private readonly AppDbContext _context;

    public DbSeeder(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        if (!await _context.Currencies.AnyAsync())
        {
            await AddCurrenciesAsync(ct);
        }

        // TODO: Add other seeding later

        await tx.CommitAsync(ct);
    }

    private async Task AddCurrenciesAsync(CancellationToken ct)
    {
        var currencies = new List<Currency>
        {
            new() { Code = "UAH", Name = "Hryvnia" },
            new() { Code = "USD", Name = "United States Dollar"},
            new() { Code = "EUR", Name = "Euro"}
        };

        _context.Currencies.AddRange(currencies);
        await _context.SaveChangesAsync(ct);
    }
}
