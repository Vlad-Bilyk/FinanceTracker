using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class FinancialOperationService : IFinancialOperationService
{
    private const string NoteSeparator = "\n--- System Info ---\n";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IValidator<FinancialOperationUpsertDto> _upsertValidator;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<FinancialOperationService> _logger;

    public FinancialOperationService(IUnitOfWork unitOfWork, IUserContext userContext,
        IValidator<FinancialOperationUpsertDto> upsertValidator, IExchangeRateService exchangeRateService,
        ILogger<FinancialOperationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _upsertValidator = upsertValidator ?? throw new ArgumentNullException(nameof(upsertValidator));
        _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FinancialOperationDetailsDto> GetOperationByIdAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        var finOperation = await GetValidOperationAsync(walletId, id, ct);
        _ = await GetValidWalletAsync(walletId, ct);

        return MapToDeatilsDto(finOperation);
    }

    public async Task<IReadOnlyList<FinancialOperationDetailsDto>> GetOperationsForWalletAsync(Guid walletId, CancellationToken ct = default)
    {
        _ = await GetValidWalletAsync(walletId, ct); // Checking that the wallet belongs to the user 

        var finOperations = await _unitOfWork.FinancialOperations.GetWalletOperationsAsync(walletId, ct);

        return finOperations.Select(MapToDeatilsDto).ToList();
    }

    public async Task<PagedResult<FinancialOperationDetailsDto>> GetUserOperationsAsync(
        OperationQuery query, CancellationToken ct = default)
    {
        const int MinPageSize = 0;
        const int MaxPageSize = 100;
        const int StandartPageSize = 20;
        const int MinPageNumber = 1;
        const int StandartPageNumber = 1;

        var userId = GetCurrentUserId();

        if (query.Page < MinPageNumber)
        {
            query.Page = StandartPageNumber;
        }

        if (query.PageSize <= MinPageSize || query.PageSize > MaxPageSize)
        {
            query.PageSize = StandartPageSize;
        }

        var pagedEntities = await _unitOfWork.FinancialOperations
            .GetUserOperationsAsync(userId, query, ct);

        var dtoItems = pagedEntities.Items
            .Select(MapToDeatilsDto).ToList();

        return new PagedResult<FinancialOperationDetailsDto>(
            dtoItems, pagedEntities.Page, pagedEntities.PageSize, pagedEntities.TotalCount);
    }

    public async Task<Guid> CreateOperationAsync(Guid walletId, FinancialOperationUpsertDto createDto, CancellationToken ct = default)
    {
        await _upsertValidator.ValidateAndThrowAsync(createDto, ct);

        var wallet = await GetValidWalletAsync(walletId, ct);
        await EnsureTypeOwnedAsync(createDto.TypeId, ct);
        await ValidateCurrencyExistsAsync(createDto.CurrencyOriginalCode!, ct);

        var exchangeRate = await _exchangeRateService.GetExchangeRateAsync(
            createDto.CurrencyOriginalCode!, wallet.BaseCurrencyCode, createDto.Date, ct);

        var amountBase = createDto.AmountOriginal * exchangeRate;

        var note = BuildOperationNote(createDto.Note, createDto.AmountOriginal,
            createDto.CurrencyOriginalCode!, wallet.BaseCurrencyCode, exchangeRate);

        var finOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            TypeId = createDto.TypeId,
            AmountBase = amountBase,
            AmountOriginal = createDto.AmountOriginal,
            CurrencyOriginalCode = createDto.CurrencyOriginalCode,
            Date = createDto.Date,
            Note = note
        };

        await _unitOfWork.FinancialOperations.AddAsync(finOperation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created financial operation with id {OperationId} in Wallet with id {WalletId}",
            finOperation.Id, walletId);

        return finOperation.Id;
    }

    public async Task UpdateOperationAsync(Guid walletId, Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct = default)
    {
        var finOperation = await GetValidOperationAsync(walletId, id, ct);

        await _upsertValidator.ValidateAndThrowAsync(updateDto, ct);

        await EnsureTypeOwnedAsync(updateDto.TypeId, ct);
        await ValidateCurrencyExistsAsync(updateDto.CurrencyOriginalCode!, ct);

        finOperation.TypeId = updateDto.TypeId;
        finOperation.Date = updateDto.Date;

        if (ShouldRecalculateExchangeRate(finOperation, updateDto))
        {
            var exchangeRate = await _exchangeRateService.GetExchangeRateAsync(
                updateDto.CurrencyOriginalCode!, finOperation.Wallet.BaseCurrencyCode, updateDto.Date, ct);

            finOperation.AmountOriginal = updateDto.AmountOriginal;
            finOperation.CurrencyOriginalCode = updateDto.CurrencyOriginalCode;
            finOperation.AmountBase = updateDto.AmountOriginal * exchangeRate;

            finOperation.Note = BuildOperationNote(updateDto.Note, finOperation.AmountOriginal,
                finOperation.CurrencyOriginalCode!, finOperation.Wallet.BaseCurrencyCode, exchangeRate);
        }
        else
        {
            finOperation.Note = updateDto.Note?.Trim();
        }

        _unitOfWork.FinancialOperations.Update(finOperation);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated financial operation with id {OperationId} in wallet with id {WalletId}",
            finOperation.Id, walletId);
    }

    public async Task SoftDeleteOperationAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        var finOperation = await GetValidOperationAsync(walletId, id, ct);

        _unitOfWork.FinancialOperations.SoftDelete(finOperation);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Soft deleted financial operation with id {OperationId} in wallet with id {WalletId}",
            finOperation.Id, walletId);
    }

    private Guid GetCurrentUserId()
    {
        return _userContext.GetRequiredUserId();
    }

    private async Task<FinancialOperation> GetValidOperationAsync(Guid walletId, Guid operationId, CancellationToken ct = default)
    {
        _ = await GetValidWalletAsync(walletId, ct); // Checking that the wallet belongs to the user 

        return await _unitOfWork.FinancialOperations.GetByIdWithDetailsAsync(walletId, operationId, ct)
            ?? throw new NotFoundException($"Financial operation with id {operationId} was not found");
    }

    private async Task<Wallet> GetValidWalletAsync(Guid walletId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return await _unitOfWork.Wallets.GetByIdForUserAsync(userId, walletId, ct)
            ?? throw new NotFoundException($"Wallet with id {walletId} was not found");
    }

    private async Task EnsureTypeOwnedAsync(Guid typeId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        _ = await _unitOfWork.FinancialOperationTypes.GetByIdForUserAsync(userId, typeId, ct)
            ?? throw new NotFoundException($"Operation type with id {typeId} was not found");
    }

    private async Task ValidateCurrencyExistsAsync(string currencyCode, CancellationToken ct)
    {
        var exists = await _unitOfWork.Currencies.ExistsAsync(currencyCode, ct);
        if (!exists)
        {
            throw new ValidationException(
                $"Currency code '{currencyCode}' is not supported. " +
                $"Use GET /api/currencies to see available currencies.");
        }
    }

    private static string BuildOperationNote(string? currentNote, decimal amountOriginal,
        string currencyOriginalCode, string baseCurrencyCode, decimal exchangeRate)
    {
        var userNote = ExtractUserNote(currentNote);

        var originalInfo =
            $"Original amount: {amountOriginal} {currencyOriginalCode}, " +
            $"exchange rate {exchangeRate:F4} {baseCurrencyCode}/{currencyOriginalCode}.";

        if (string.IsNullOrEmpty(userNote))
        {
            return originalInfo;
        }

        return $"{userNote}{NoteSeparator}{originalInfo}";
    }

    private static string ExtractUserNote(string? currentNote)
    {
        if (string.IsNullOrWhiteSpace(currentNote))
        {
            return string.Empty;
        }

        var separatorIndex = currentNote.IndexOf(NoteSeparator);
        return separatorIndex > 0
            ? currentNote[..separatorIndex].Trim()
            : currentNote;
    }

    private static bool ShouldRecalculateExchangeRate(FinancialOperation existing, FinancialOperationUpsertDto dto)
    {
        return existing.AmountOriginal != dto.AmountOriginal
            || !string.Equals(existing.CurrencyOriginalCode, dto.CurrencyOriginalCode, StringComparison.OrdinalIgnoreCase)
            || existing.Date != dto.Date;
    }

    private static FinancialOperationDetailsDto MapToDeatilsDto(FinancialOperation operation)
    {
        return new FinancialOperationDetailsDto(
            operation.Id,
            operation.TypeId,
            operation.Type.Name,
            operation.Type.Kind,
            operation.WalletId,
            operation.Wallet.Name,
            operation.AmountBase,
            operation.AmountOriginal,
            operation.CurrencyOriginalCode,
            operation.Date,
            operation.Note
        );
    }
}
