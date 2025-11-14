using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FindAsync([id], ct);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.UserName == userName, ct);
    }

    public async Task AddAsync(User entity, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(entity, ct);
    }

    public void Update(User entity)
    {
        _context.Users.Update(entity);
    }

    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
    }
}
