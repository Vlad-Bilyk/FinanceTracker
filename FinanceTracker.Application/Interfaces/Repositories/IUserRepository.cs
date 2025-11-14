using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task AddAsync(User entity, CancellationToken ct = default);
    void Update(User entity);
    void Delete(User entity);
}
