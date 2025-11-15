using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Wallet?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<Wallet>> GetUserWalletsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Wallets
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeWalletId, CancellationToken ct)
    {
        var normalizedName = name.Trim();

        var query = _context.Wallets
            .Where(w => w.UserId == userId
                     && w.Name == normalizedName);

        if (excludeWalletId.HasValue)
        {
            query = query.Where(w => w.Id != excludeWalletId.Value);
        }

        return query.AnyAsync(ct);
    }

    public async Task AddAsync(Wallet entity, CancellationToken ct = default)
    {
        await _context.Wallets.AddAsync(entity, ct);
    }

    public void Update(Wallet entity)
    {
        _context.Wallets.Update(entity);
    }

    public void Delete(Wallet entity)
    {
        _context.Wallets.Remove(entity);
    }
}
