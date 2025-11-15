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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IValidator<FinancialOperationUpsertDto> _upsertValidator;
    private readonly ILogger<FinancialOperationService> _logger;

    public FinancialOperationService(IUnitOfWork unitOfWork, IUserContext userContext,
        IValidator<FinancialOperationUpsertDto> upsertValidator, ILogger<FinancialOperationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _upsertValidator = upsertValidator ?? throw new ArgumentNullException(nameof(upsertValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FinancialOperationDetailsDto> GetOperationByIdAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        var finOperation = await _unitOfWork.FinancialOperations.GetByIdWithDetailsAsync(walletId, id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        // TODO: add mapping with AutoMapper
        return new FinancialOperationDetailsDto
        (
            finOperation.Id, finOperation.TypeId, finOperation.Type.Name, finOperation.Type.Kind,
            finOperation.WalletId, finOperation.Wallet.Name, finOperation.AmountBase, finOperation.AmountOriginal,
            finOperation.CurrencyOriginalCode, finOperation.Date, finOperation.Note
        );
    }

    public async Task<IReadOnlyList<FinancialOperationDetailsDto>> GetAllOperationsAsync(Guid walletId, CancellationToken ct = default)
    {
        var finOperations = await _unitOfWork.FinancialOperations.GetWalletOperationsAsync(walletId, ct);

        return finOperations.Select(op => new FinancialOperationDetailsDto
        (
            op.Id, op.TypeId, op.Type.Name, op.Type.Kind,
            op.WalletId, op.Wallet.Name, op.AmountBase, op.AmountOriginal,
            op.CurrencyOriginalCode, op.Date, op.Note
        )).ToList();
    }

    public async Task<Guid> CreateOperationAsync(Guid walletId, FinancialOperationUpsertDto createDto, CancellationToken ct = default)
    {
        await _upsertValidator.ValidateAndThrowAsync(createDto, ct);

        await EnshureWalletOwnedAsync(walletId, ct);
        await EnshureTypeOwnedAsync(createDto.TypeId, ct);

        var finOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            TypeId = createDto.TypeId,
            AmountBase = 0, // TODO: This should be calculated based on currency conversion logic 
            AmountOriginal = createDto.AmountOriginal,
            CurrencyOriginalCode = createDto.CurrencyOriginalCode,
            Date = createDto.Date,
            Note = createDto.Note // TODO: write exchange rate in the Note field
        };

        await _unitOfWork.FinancialOperations.AddAsync(finOperation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created financial operation with id {OperationId} in Wallet with id {WalletId}",
            finOperation.Id, walletId);

        return finOperation.Id;
    }

    public async Task UpdateOperationAsync(Guid walletId, Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct = default)
    {
        var finOperation = await _unitOfWork.FinancialOperations.GetByIdAsync(walletId, id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        await _upsertValidator.ValidateAndThrowAsync(updateDto, ct);

        await EnshureTypeOwnedAsync(updateDto.TypeId, ct);

        finOperation.TypeId = updateDto.TypeId;
        finOperation.AmountOriginal = updateDto.AmountOriginal;
        finOperation.Date = updateDto.Date;
        finOperation.Note = updateDto.Note;

        _unitOfWork.FinancialOperations.Update(finOperation);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated financial operation with id {OperationId} in wallet with id {WalletId}",
            finOperation.Id, walletId);
    }

    public async Task SoftDeleteOperationAsync(Guid walletId, Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperations.GetByIdAsync(walletId, id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        _unitOfWork.FinancialOperations.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Soft deleted financial operation with id {OperationId} in wallet with id {WalletId}",
            entity.Id, walletId);
    }

    private async Task EnshureWalletOwnedAsync(Guid walletId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        _ = await _unitOfWork.Wallets.GetByIdForUserAsync(userId, walletId, ct)
            ?? throw new NotFoundException($"Wallet with id {walletId} was not found");
    }

    private async Task EnshureTypeOwnedAsync(Guid typeId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        _ = await _unitOfWork.FinancialOperationTypes.GetByIdForUserAsync(userId, typeId, ct)
            ?? throw new NotFoundException($"Operation type with id {typeId} was not found");
    }

    private Guid GetCurrentUserId()
    {
        return _userContext.GetRequiredUserId();
    }
}
