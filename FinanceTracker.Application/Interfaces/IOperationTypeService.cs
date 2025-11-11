using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces;

public interface IOperationTypeService
{
    Task<OperationTypeDto> GetTypeByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OperationTypeDto>> GetAllTypesAsync(CancellationToken ct = default);
    Task<Guid> CreateTypeAsync(OperationTypeCreateDto createDto, CancellationToken ct = default);
    Task UpdateTypeAsync(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct = default);
    Task DeleteTypeAsync(Guid id, CancellationToken ct = default);
}
