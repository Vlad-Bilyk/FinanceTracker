using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
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

    public async Task<FinancialOperationType?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        return await _context.FinancialOperationTypes
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<FinancialOperationType>> GetUserTypesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.FinancialOperationTypes
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Kind)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FinancialOperationType entity, CancellationToken ct = default)
    {
        await _context.FinancialOperationTypes.AddAsync(entity, ct);
    }

    public async Task<bool> ExistsByNameKindAsync(Guid userId, string name, OperationKind kind,
        Guid? excludeTypeId, CancellationToken ct = default)
    {
        var normalizedName = name.Trim();

        var query = _context.FinancialOperationTypes
            .Where(x => x.UserId == userId
                        && x.Kind == kind
                        && x.Name == normalizedName);

        if (excludeTypeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeTypeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public void Update(FinancialOperationType entity)
    {
        _context.FinancialOperationTypes.Update(entity);
    }

    public void Delete(FinancialOperationType entity)
    {
        _context.Remove(entity);
    }
}
