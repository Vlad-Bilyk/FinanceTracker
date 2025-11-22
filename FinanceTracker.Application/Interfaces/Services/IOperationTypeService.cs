using FinanceTracker.Application.DTOs.OperationType;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IOperationTypeService
{
    Task<OperationTypeDto> GetTypeByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OperationTypeDto>> GetUserTypesAsync(CancellationToken ct = default);
    Task<Guid> CreateTypeAsync(OperationTypeCreateDto createDto, CancellationToken ct = default);
    Task UpdateTypeAsync(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct = default);

    /// <summary>
    /// Deletes an operation type. If type is used, should set replacement type
    /// </summary>
    /// <param name="id">Operation type identifier.</param>
    /// <param name="replacementTypeId">Optional identifier of the replacement operation type.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteTypeAsync(Guid id, Guid? replacementTypeId, CancellationToken ct = default);
}
