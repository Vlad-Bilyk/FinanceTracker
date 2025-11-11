using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Infrastructure.Data;

namespace FinanceTracker.Infrastructure.Repositories;

/// <summary>
/// Implementation of Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IFinancialOperationTypeRepository FinancialOperationTypes { get; }
    public IFinancialOperationRepository FinancialOperations { get; }

    public UnitOfWork(
        AppDbContext context,
        IFinancialOperationTypeRepository typeRepository,
        IFinancialOperationRepository operationRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        FinancialOperationTypes = typeRepository ?? throw new ArgumentNullException(nameof(typeRepository));
        FinancialOperations = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
