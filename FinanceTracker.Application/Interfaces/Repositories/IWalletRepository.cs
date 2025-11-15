using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Wallet>> GetUserWalletsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeWalletId, CancellationToken ct);
    Task AddAsync(Wallet entity, CancellationToken ct = default);
    void Update(Wallet entity);
    void Delete(Wallet entity);
}
