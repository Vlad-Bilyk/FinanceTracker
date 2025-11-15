using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
}
