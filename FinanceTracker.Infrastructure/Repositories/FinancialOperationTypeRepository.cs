using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class FinancialOperationTypeRepository : IFinancialOperationTypeRepository
{
    private readonly AppDbContext _context;

    public FinancialOperationTypeRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<FinancialOperationType?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperationTypes.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<FinancialOperationType>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.FinancialOperationTypes
            .AsNoTracking()
            .OrderBy(x => x.Kind)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FinancialOperationType entity, CancellationToken ct = default)
    {
        await _context.FinancialOperationTypes.AddAsync(entity, ct);
    }

    public void Update(FinancialOperationType entity)
    {
        _context.FinancialOperationTypes.Update(entity);
    }

    public void Delete(FinancialOperationType entity)
    {
        _context.FinancialOperationTypes.Remove(entity);
    }
}
