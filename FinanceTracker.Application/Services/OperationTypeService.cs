using FinanceTracker.Application.DTOs.OperationType;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class OperationTypeService : IOperationTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IValidator<OperationTypeCreateDto> _createValidator;
    private readonly IValidator<OperationTypeUpdateDto> _updateValidator;
    private readonly ILogger<OperationTypeService> _logger;

    public OperationTypeService(IUnitOfWork unitOfWork, IValidator<OperationTypeCreateDto> createValidator,
        IUserContext userContext, IValidator<OperationTypeUpdateDto> updateValidator,
        ILogger<OperationTypeService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationTypeDto> GetTypeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetValidTypeAsync(id, ct);
        return new OperationTypeDto(entity.Id, entity.Name, entity.Description, entity.Kind);
    }

    public async Task<IReadOnlyList<OperationTypeDto>> GetUserTypesAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var entities = await _unitOfWork.FinancialOperationTypes.GetUserTypesAsync(userId, ct);

        return entities.Select(e => new OperationTypeDto(e.Id, e.Name, e.Description, e.Kind)).ToList();
    }

    public async Task<Guid> CreateTypeAsync(OperationTypeCreateDto createDto, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(createDto, ct);

        await ValidateUniqueNameAsync(createDto.Name, createDto.Kind, null, ct);

        var entity = new FinancialOperationType
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.GetRequiredUserId(),
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

        var type = await GetValidTypeAsync(id, ct);

        await ValidateUniqueNameAsync(updateDto.Name, type.Kind, id, ct);

        type.Name = updateDto.Name.Trim();
        type.Description = updateDto.Description;

        _unitOfWork.FinancialOperationTypes.Update(type);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated financial operation type with id {OperationTypeId}", id);
    }

    public async Task DeleteTypeAsync(Guid id, CancellationToken ct = default)
    {
        var type = await GetValidTypeAsync(id, ct);

        _unitOfWork.FinancialOperationTypes.SoftDelete(type);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted financial operation type with id {OperationTypeId}", id);
    }

    private async Task<FinancialOperationType> GetValidTypeAsync(Guid id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var type = await _unitOfWork.FinancialOperationTypes.GetByIdForUserAsync(userId, id, ct)
            ?? throw new NotFoundException($"Financial operation type with id {id} not found");
        return type;
    }

    private Guid GetCurrentUserId()
    {
        return _userContext.GetRequiredUserId();
    }

    private async Task ValidateUniqueNameAsync(string name, OperationKind kind, Guid? excludeTypeId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var exists = await _unitOfWork.FinancialOperationTypes
            .ExistsByNameKindAsync(userId, name, kind, excludeTypeId, ct);
        if (exists)
        {
            throw new ConflictException("Operation type with the same Name and Kind already exists.");
        }
    }
}
