using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.DTOs.Report;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, IUserContext userContext,
        ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FinanceReportDto> CreateDailyReportAsync(
        Guid walletId, DateOnly date, CancellationToken ct = default)
    {
        _ = await GetValidWalletAsync(walletId, ct);
        var operations = await GetOperationsAsync(walletId, date, ct);

        _logger.LogInformation("Generated daily report: Wallet={WalletId}, Date={Date}, Operations={Count}",
            walletId, date, operations.Count);

        return BuidFinanceReportDto(operations);
    }

    public async Task<FinanceReportDto> CreatePeriodReportAsync(
        Guid walletId, DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        if (start > end)
        {
            throw new ValidationException(errors:
            [
                new ValidationFailure("'start' or 'end' date", "End date must be after or equal to start date")
            ]);
        }

        _ = await GetValidWalletAsync(walletId, ct);
        var operations = await GetOperationsAsync(walletId, start, end, ct);

        _logger.LogInformation("Generated period report from {StartDate} to {EndDate} " +
            "with {OperationCount} operations in wallet {WalletId}",
            start, end, operations.Count, walletId);

        return BuidFinanceReportDto(operations);
    }

    private async Task<List<FinancialOperationDetailsDto>> GetOperationsAsync(
        Guid walletId, DateOnly date, CancellationToken ct)
    {
        var entities = await _unitOfWork.FinancialOperations
            .GetListByDateAsync(walletId, date, ct);
        return MapToDto(entities);
    }

    private async Task<List<FinancialOperationDetailsDto>> GetOperationsAsync(
        Guid walletId, DateOnly start, DateOnly end, CancellationToken ct)
    {
        var entities = await _unitOfWork.FinancialOperations
            .GetListByPeriodAsync(walletId, start, end, ct);
        return MapToDto(entities);
    }

    private static List<FinancialOperationDetailsDto> MapToDto(
        IEnumerable<FinancialOperation> entities)
    {
        return entities
            .Select(op => new FinancialOperationDetailsDto(
                op.Id, op.TypeId, op.Type.Name, op.Type.Kind,
                op.WalletId, op.Wallet.Name, op.AmountBase, op.AmountOriginal,
                op.CurrencyOriginalCode, op.Date, op.Note
            )).ToList();
    }

    private static decimal CalculateTotal(
        IEnumerable<FinancialOperationDetailsDto> operations, OperationKind kind)
    {
        return operations
            .Where(x => x.Kind == kind)
            .Sum(x => x.AmountBase);
    }

    private static List<CategoryAmountDto> CalculateSumByCategory(
        IEnumerable<FinancialOperationDetailsDto> operations, OperationKind kind)
    {
        return operations
            .Where(x => x.Kind == kind)
            .GroupBy(x => new { x.TypeId, x.TypeName })
            .Select(g => new CategoryAmountDto
            {
                TypeId = g.Key.TypeId,
                TypeName = g.Key.TypeName,
                Amount = g.Sum(x => x.AmountBase)
            }).ToList();
    }

    private static FinanceReportDto BuidFinanceReportDto(IEnumerable<FinancialOperationDetailsDto> operations)
    {
        return new FinanceReportDto
        {
            TotalIncome = CalculateTotal(operations, OperationKind.Income),
            TotalExpense = CalculateTotal(operations, OperationKind.Expense),
            IncomeByCategory = CalculateSumByCategory(operations, OperationKind.Income),
            ExpensesByCategory = CalculateSumByCategory(operations, OperationKind.Expense)
        };
    }

    private async Task<Wallet> GetValidWalletAsync(Guid walletId, CancellationToken ct)
    {
        var userId = _userContext.GetRequiredUserId();
        var wallet = await _unitOfWork.Wallets.GetByIdForUserAsync(userId, walletId, ct)
            ?? throw new NotFoundException($"Wallet with id {walletId} was not found");

        return wallet;
    }
}
