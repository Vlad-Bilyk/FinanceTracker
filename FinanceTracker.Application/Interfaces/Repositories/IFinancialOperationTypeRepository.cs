using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationTypeRepository
{
    Task<FinancialOperationType?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperationType>> GetUserTypesAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(FinancialOperationType entity, CancellationToken ct = default);
    Task<bool> ExistsByNameKindAsync(Guid userId, string name, OperationKind kind, Guid? excludeTypeId, CancellationToken ct);
    void Update(FinancialOperationType entity);
    void SoftDelete(FinancialOperationType entity);
}
