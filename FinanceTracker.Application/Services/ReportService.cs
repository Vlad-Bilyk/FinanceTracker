using FinanceTracker.Application.DTOs;
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
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FinanceReport> CreateDailyReportAsync(
        DateOnly date, CancellationToken ct = default)
    {
        var operations = await GetOperationsAsync(date, ct);

        _logger.LogInformation("Generated daily report for date {Date} with {OperationCount} operations", date, operations.Count);
        return new FinanceReport
        {
            Start = date,
            End = date,
            TotalIncome = CalculateTotalIncome(operations),
            TotalExpense = CalculateTotalExpense(operations),
            Operations = operations
        };
    }

    public async Task<FinanceReport> CreatePeriodReportAsync(
        DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        if (start > end)
        {
            throw new ValidationException(errors:
            [
                new ValidationFailure(nameof(start) + nameof(end), "End date must be after or equal to start date")
            ]);
        }

        var operations = await GetOperationsAsync(start, end, ct);

        _logger.LogInformation("Generated period report from {StartDate} to {EndDate} with {OperationCount} operations",
            start, end, operations.Count);
        return new FinanceReport
        {
            Start = start,
            End = end,
            TotalIncome = CalculateTotalIncome(operations),
            TotalExpense = CalculateTotalExpense(operations),
            Operations = operations
        };
    }

    private async Task<List<FinancialOperationDetailsDto>> GetOperationsAsync(
        DateOnly date, CancellationToken ct)
    {
        var entities = await _unitOfWork.FinancialOperations.GetListByDateAsync(date, ct);
        return MapToDto(entities);
    }

    private async Task<List<FinancialOperationDetailsDto>> GetOperationsAsync(
        DateOnly start, DateOnly end, CancellationToken ct)
    {
        var entities = await _unitOfWork.FinancialOperations.GetListByPeriodAsync(start, end, ct);
        return MapToDto(entities);
    }

    private static List<FinancialOperationDetailsDto> MapToDto(
        IEnumerable<FinancialOperation> entities)
    {
        return entities
            .Select(e => new FinancialOperationDetailsDto(
                e.Id,
                e.TypeId,
                e.Type.Name,
                e.Type.Kind,
                e.AmountBase,
                e.Date,
                e.Note))
            .ToList();
    }

    private static decimal CalculateTotalIncome(IEnumerable<FinancialOperationDetailsDto> operations)
    {
        return operations
            .Where(x => x.Kind == OperationKind.Income)
            .Sum(x => x.Amount);
    }

    private static decimal CalculateTotalExpense(IEnumerable<FinancialOperationDetailsDto> operations)
    {
        return operations
            .Where(x => x.Kind == OperationKind.Expense)
            .Sum(x => x.Amount);
    }
}
