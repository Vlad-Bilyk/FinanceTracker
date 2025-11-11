using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationTypeRepository
{
    Task<FinancialOperationType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperationType>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(FinancialOperationType entity, CancellationToken ct = default);
    Task<bool> ExistsByNameKindAsync(Guid? excludeId, string name, OperationKind kind, CancellationToken ct);
    void Update(FinancialOperationType entity);
    void Delete(FinancialOperationType entity);
}
