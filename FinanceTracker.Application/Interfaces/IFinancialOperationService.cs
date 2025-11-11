using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces;

public interface IFinancialOperationService
{
    Task<FinancialOperationDetailsDto> GetOperationByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperationDetailsDto>> GetAllOperationsAsync(CancellationToken ct = default);
    Task<Guid> CreateOperationAsync(FinancialOperationUpsertDto createDto, CancellationToken ct = default);
    Task UpdateOperationAsync(Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct = default);
    Task SoftDeleteOperationAsync(Guid id, CancellationToken ct = default);
}
