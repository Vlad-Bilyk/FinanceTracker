using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationRepository
{
    Task<FinancialOperation?> GetByIdAsync(Guid walletId, Guid id, CancellationToken ct = default);
    Task<FinancialOperation?> GetByIdWithDetailsAsync(Guid walletId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetWalletOperationsAsync(Guid walletId, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByDateAsync(Guid walletId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByPeriodAsync(Guid walletId, DateOnly start, DateOnly end, CancellationToken ct = default);
    Task AddAsync(FinancialOperation entity, CancellationToken ct = default);
    Task<bool> AnyByTypeIdAsync(Guid typeId, CancellationToken ct);
    void Update(FinancialOperation entity);

    /// <summary>
    /// Soft delete: mark IsDeleted = true
    /// </summary>
    void SoftDelete(FinancialOperation entity);
}
