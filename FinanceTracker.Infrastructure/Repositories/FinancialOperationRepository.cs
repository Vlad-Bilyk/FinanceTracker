using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class FinancialOperationRepository : IFinancialOperationRepository
{
    private readonly AppDbContext _context;

    public FinancialOperationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<FinancialOperation?> GetByIdAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .FirstOrDefaultAsync(x => x.Id == id && x.WalletId == walletId, ct);
    }

    public async Task<FinancialOperation?> GetByIdWithDetailsAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .Include(x => x.Type)
            .Include(x => x.Wallet)
            .FirstOrDefaultAsync(x => x.Id == id && x.WalletId == walletId, ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetWalletOperationsAsync(Guid walletId, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .AsNoTracking()
            .Include(x => x.Type)
            .Include(x => x.Wallet)
            .Where(x => x.WalletId == walletId)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetListByDateAsync(
        Guid walletId, DateOnly date, CancellationToken ct = default)
    {
        return await GetOperationsInRangeAsync(walletId, date, date, ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetListByPeriodAsync(
        Guid walletId, DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        return await GetOperationsInRangeAsync(walletId, start, end, ct);
    }

    public async Task AddAsync(FinancialOperation entity, CancellationToken ct = default)
    {
        await _context.FinancialOperations.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetByTypeIdAsync(Guid typeId, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .Where(x => x.TypeId == typeId)
            .ToListAsync(ct);
    }

    public async Task<bool> AnyByTypeIdAsync(Guid typeId, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .AnyAsync(x => x.TypeId == typeId, ct);
    }

    /// <inheritdoc/>
    public void SoftDelete(FinancialOperation entity)
    {
        entity.IsDeleted = true;
        _context.FinancialOperations.Update(entity);
    }

    public void Update(FinancialOperation entity)
    {
        _context.FinancialOperations.Update(entity);
    }

    private async Task<IReadOnlyList<FinancialOperation>> GetOperationsInRangeAsync(
        Guid walletId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var start = ToDateTime(from);
        var end = ToDateTime(to.AddDays(1));

        return await _context.FinancialOperations
            .AsNoTracking()
            .Include(x => x.Type)
            .Include(x => x.Wallet)
            .Where(x => x.WalletId == walletId && (x.Date >= start && x.Date < end))
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    private static DateTime ToDateTime(DateOnly date)
    {
        return date.ToDateTime(TimeOnly.MinValue);
    }
}
