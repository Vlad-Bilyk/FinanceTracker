using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class OperationTypeService : IOperationTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<OperationTypeCreateDto> _createValidator;
    private readonly IValidator<OperationTypeUpdateDto> _updateValidator;
    private readonly ILogger<OperationTypeService> _logger;

    public OperationTypeService(IUnitOfWork unitOfWork, IValidator<OperationTypeCreateDto> createValidator,
        IValidator<OperationTypeUpdateDto> updateValidator, ILogger<OperationTypeService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationTypeDto> GetTypeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperationTypes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation type with id {id} not found");

        return new OperationTypeDto(entity.Id, entity.Name, entity.Description, entity.Kind);
    }

    public async Task<IReadOnlyList<OperationTypeDto>> GetAllTypesAsync(CancellationToken ct = default)
    {
        var entities = await _unitOfWork.FinancialOperationTypes.GetAllAsync(ct);

        return entities.Select(e => new OperationTypeDto(e.Id, e.Name, e.Description, e.Kind)).ToList();
    }

    public async Task<Guid> CreateTypeAsync(OperationTypeCreateDto createDto, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(createDto, ct);

        var exists = await _unitOfWork.FinancialOperationTypes
            .ExistsByNameKindAsync(excludeId: null, createDto.Name, createDto.Kind, ct);
        if (exists)
        {
            throw new ConflictException("Operation type with the same Name and Kind already exists.");
        }

        var entity = new FinancialOperationType
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name.Trim(),
            Description = createDto.Description,
            Kind = createDto.Kind,
        };

        await _unitOfWork.FinancialOperationTypes.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);


        _logger.LogInformation("Created financial operation type with id {OperationTypeId}", entity.Id);
        return entity.Id;
    }

    public async Task UpdateTypeAsync(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(updateDto, ct);

        var entity = await _unitOfWork.FinancialOperationTypes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation type with id {id} not found");

        var newName = updateDto.Name.Trim();

        if (!newName.Equals(entity.Name, StringComparison.Ordinal))
        {
            var exists = await _unitOfWork.FinancialOperationTypes
                .ExistsByNameKindAsync(excludeId: id, newName, entity.Kind, ct);
            if (exists)
            {
                throw new ConflictException("Operation type with the same Name and Kind already exists.");
            }

            entity.Name = newName;
        }

        entity.Description = updateDto.Description;

        _unitOfWork.FinancialOperationTypes.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated financial operation type with id {OperationTypeId}", id);
    }

    public async Task DeleteTypeAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FinancialOperationTypes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Financial operation type with id {id} not found");

        var isUsed = await _unitOfWork.FinancialOperations.AnyByTypeIdAsync(id, ct);

        if (isUsed)
        {
            throw new ConflictException("Cannot delete operation type because it is used in existing operations.");
        }

        _unitOfWork.FinancialOperationTypes.Delete(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted financial operation type with id {OperationTypeId}", id);
    }
}
