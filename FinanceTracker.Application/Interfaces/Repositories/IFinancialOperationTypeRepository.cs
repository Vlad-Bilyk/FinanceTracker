using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationTypeRepository
{
    Task<FinancialOperationType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperationType>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(FinancialOperationType entity, CancellationToken ct = default);
    void Update(FinancialOperationType entity);
    void Delete(FinancialOperationType entity);
}
