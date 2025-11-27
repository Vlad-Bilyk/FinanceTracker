using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _context;

    public CurrencyRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Currencies
            .FindAsync([code], ct);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken ct = default)
    {
        return await _context.Currencies
            .AnyAsync(c => c.Code == code, ct);
    }
}
