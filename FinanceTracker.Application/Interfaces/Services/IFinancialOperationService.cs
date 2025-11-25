using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.DTOs.Operation;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IFinancialOperationService
{
    Task<FinancialOperationDetailsDto> GetOperationByIdAsync(Guid walletId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialOperationDetailsDto>> GetOperationsForWalletAsync(Guid walletId, CancellationToken ct = default);
    Task<PagedResult<FinancialOperationDetailsDto>> GetUserOperationsAsync(OperationQuery query, CancellationToken ct = default);
    Task<Guid> CreateOperationAsync(Guid walletId, FinancialOperationUpsertDto createDto, CancellationToken ct = default);
    Task UpdateOperationAsync(Guid walletId, Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct = default);
    Task SoftDeleteOperationAsync(Guid walletId, Guid id, CancellationToken ct = default);
}
