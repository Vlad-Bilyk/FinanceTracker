using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationRepository
{
    Task<FinancialOperation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FinancialOperation?> GetByIdWithTypeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetAllWithTypeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByPeriodAsync(DateOnly start, DateOnly end, CancellationToken ct = default);
    Task AddAsync(FinancialOperation entity, CancellationToken ct = default);
    void Update(FinancialOperation entity);

    /// <summary>
    /// Soft delete: mark IsDeleted = true
    /// </summary>
    void SoftDelete(FinancialOperation entity);
}
