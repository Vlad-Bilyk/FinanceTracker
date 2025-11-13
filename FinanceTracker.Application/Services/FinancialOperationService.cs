using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class FinancialOperationService : IFinancialOperationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<FinancialOperationUpsertDto> _upsertValidator;
    private readonly ILogger<FinancialOperationService> _logger;

    public FinancialOperationService(IUnitOfWork unitOfWork, IValidator<FinancialOperationUpsertDto> upsertValidator,
        ILogger<FinancialOperationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _upsertValidator = upsertValidator ?? throw new ArgumentNullException(nameof(upsertValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FinancialOperationDetailsDto> GetOperationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperations.GetByIdWithTypeAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        _logger.LogInformation("Retrieved financial operation with id {OperationId}", entity.Id);
        return new FinancialOperationDetailsDto(entity.Id, entity.TypeId, entity.Type.Name,
            entity.Type.Kind, entity.AmountBase, entity.Date, entity.Note);
    }

    public async Task<IReadOnlyList<FinancialOperationDetailsDto>> GetAllOperationsAsync(CancellationToken ct = default)
    {
        var entities = await _unitOfWork.FinancialOperations.GetAllWithTypeAsync(ct);

        _logger.LogInformation("Retrieved all financial operations");
        return entities.Select(e => new FinancialOperationDetailsDto(
            e.Id, e.TypeId, e.Type.Name, e.Type.Kind, e.AmountBase, e.Date, e.Note
            )).ToList();
    }

    public async Task<Guid> CreateOperationAsync(FinancialOperationUpsertDto createDto, CancellationToken ct = default)
    {
        await _upsertValidator.ValidateAndThrowAsync(createDto, ct);

        _ = await _unitOfWork.FinancialOperationTypes.GetByIdAsync(createDto.TypeId, ct)
            ?? throw new NotFoundException($"Operation type with id {createDto.TypeId} was not found");

        var entity = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            TypeId = createDto.TypeId,
            AmountBase = createDto.Amount,
            Date = createDto.Date,
            Note = createDto.Note,
        };

        await _unitOfWork.FinancialOperations.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created financial operation with id {OperationId}", entity.Id);
        return entity.Id;
    }

    public async Task UpdateOperationAsync(Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        await _upsertValidator.ValidateAndThrowAsync(updateDto, ct);

        _ = await _unitOfWork.FinancialOperationTypes.GetByIdAsync(updateDto.TypeId, ct)
            ?? throw new NotFoundException($"Operation type with id {updateDto.TypeId} was not found");

        entity.TypeId = updateDto.TypeId;
        entity.AmountBase = updateDto.Amount;
        entity.Date = updateDto.Date;
        entity.Note = updateDto.Note;

        _unitOfWork.FinancialOperations.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated financial operation with id {OperationId}", entity.Id);
    }

    public async Task SoftDeleteOperationAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation with id {id} was not found");

        _unitOfWork.FinancialOperations.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Soft deleted financial operation with id {OperationId}", entity.Id);
    }
}
