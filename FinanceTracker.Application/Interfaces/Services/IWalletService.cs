using FinanceTracker.Application.DTOs.Wallet;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IWalletService
{
    Task<WalletDto> GetWalletByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all wallets for the currently authenticated user
    /// </summary>
    Task<IReadOnlyList<WalletDto>> GetUserWalletsAsync(CancellationToken ct = default);
    Task<Guid> CreateWalletAsync(WalletCreateDto createDto, CancellationToken ct = default);
    Task UpdateWalletAsync(Guid id, WalletUpdateDto updateDto, CancellationToken ct = default);
    Task DeleteWalletAsync(Guid id, CancellationToken ct = default);
}
