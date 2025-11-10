using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IFinancialOperationRepository
{
    Task<FinancialOperation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperation>> GetListByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task AddAsync(FinancialOperation entity, CancellationToken ct = default);
    void Update(FinancialOperation entity);
    void SoftDelete(FinancialOperation entity);
}
