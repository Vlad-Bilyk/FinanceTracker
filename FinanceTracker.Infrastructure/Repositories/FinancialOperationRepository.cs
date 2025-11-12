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

    public async Task<FinancialOperation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperations.FindAsync([id], ct);
    }

    public async Task<FinancialOperation?> GetByIdWithTypeAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .AsNoTracking()
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetAllWithTypeAsync(CancellationToken ct = default)
    {
        return await _context.FinancialOperations
            .AsNoTracking()
            .Include(x => x.Type)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetListByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        return await GetOperationsInRangeAsync(date, date, ct);
    }

    public async Task<IReadOnlyList<FinancialOperation>> GetListByPeriodAsync(DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        return await GetOperationsInRangeAsync(start, end, ct);
    }

    public async Task AddAsync(FinancialOperation entity, CancellationToken ct = default)
    {
        await _context.FinancialOperations.AddAsync(entity, ct);
    }

    public async Task<bool> AnyByTypeIdAsync(Guid typeId, CancellationToken ct)
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

    private async Task<IReadOnlyList<FinancialOperation>> GetOperationsInRangeAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var start = ToUtcDateTimeOffset(from);
        var end = ToUtcDateTimeOffset(to.AddDays(1));

        return await _context.FinancialOperations
            .AsNoTracking()
            .Include(x => x.Type)
            .Where(x => x.Date >= start && x.Date < end)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    private static DateTimeOffset ToUtcDateTimeOffset(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}
