using FinanceTracker.Application.DTOs.Wallet;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IValidator<WalletCreateDto> _createValidator;
    private readonly IValidator<WalletUpdateDto> _updateValidator;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IUnitOfWork unitOfWork, IUserContext userContext,
        IValidator<WalletCreateDto> createValidator, IValidator<WalletUpdateDto> updateValidator,
        ILogger<WalletService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WalletDto> GetWalletByIdAsync(Guid id, CancellationToken ct = default)
    {
        var wallet = await GetValidWalletAsync(id, ct);

        return new WalletDto(wallet.Id, wallet.Name, wallet.BaseCurrencyCode);
    }

    public async Task<IReadOnlyList<WalletDto>> GetUserWalletsAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        var wallets = await _unitOfWork.Wallets.GetUserWalletsAsync(userId, ct);
        return wallets.Select(w => new WalletDto(
                w.Id, w.Name, w.BaseCurrencyCode)).ToList();
    }

    public async Task<Guid> CreateWalletAsync(WalletCreateDto createDto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _createValidator.ValidateAndThrowAsync(createDto, ct);

        await ValidateUniqueNameAsync(createDto.Name, null, ct);
        await ValidateCurrencyExistsAsync(createDto.BaseCurrencyCode, ct);

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = createDto.Name.Trim(),
            BaseCurrencyCode = createDto.BaseCurrencyCode
        };

        await _unitOfWork.Wallets.AddAsync(wallet, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet created: {WalletId}, Name: {WalletName}", wallet.Id, wallet.Name);
        return wallet.Id;
    }

    public async Task UpdateWalletAsync(Guid id, WalletUpdateDto updateDto, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(updateDto, ct);

        var wallet = await GetValidWalletAsync(id, ct);
        await ValidateUniqueNameAsync(updateDto.Name, id, ct);

        wallet.Name = updateDto.Name.Trim();

        _unitOfWork.Wallets.Update(wallet);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet with id {WalletId} was updated", wallet.Id);
    }

    public async Task DeleteWalletAsync(Guid id, CancellationToken ct = default)
    {
        var wallet = await GetValidWalletAsync(id, ct);

        _unitOfWork.Wallets.SoftDelete(wallet);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet with id {WalletId} was deleted", wallet.Id);
    }

    private async Task<Wallet> GetValidWalletAsync(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var wallet = await _unitOfWork.Wallets.GetByIdForUserAsync(userId, id, ct)
            ?? throw new NotFoundException($"Wallet with id {id} was not found");
        return wallet;
    }

    private async Task ValidateUniqueNameAsync(string name, Guid? excludeWalletId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var exists = await _unitOfWork.Wallets.ExistsByNameAsync(userId, name, excludeWalletId, ct);
        if (exists)
        {
            throw new ConflictException($"Wallet with name '{name}' already exists for this user");
        }
    }

    private async Task ValidateCurrencyExistsAsync(string currencyCode, CancellationToken ct)
    {
        var exists = await _unitOfWork.Currencies.ExistsAsync(currencyCode, ct);
        if (!exists)
        {
            throw new ConflictException(
                $"Currency code '{currencyCode}' is not supported. " +
                $"Use GET /api/currencies to see available currencies.");
        }
    }

    private Guid GetCurrentUserId()
    {
        return _userContext.GetRequiredUserId();
    }
}
