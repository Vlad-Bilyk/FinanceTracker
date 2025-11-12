using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace FinanceTracker.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<FinanceReport> CreateDailyReportAsync(
        DateOnly date, CancellationToken ct = default)
    {
        var operations = await GetOperationsAsync(date, ct);

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
                new ValidationFailure(nameof(start) + nameof(end), "")
            ]);
        }

        var operations = await GetOperationsAsync(start, end, ct);

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
                e.Amount,
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
