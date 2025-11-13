using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceTracker.Tests.Services;

public class OperationTypeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFinancialOperationTypeRepository> _opTypeRepositoryMock;
    private readonly Mock<IFinancialOperationRepository> _opRepositoryMock;
    private readonly Mock<IValidator<OperationTypeCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<OperationTypeUpdateDto>> _updateValidatorMock;
    private readonly Mock<ILogger<OperationTypeService>> _loggerMock;
    private readonly OperationTypeService _sut;

    public OperationTypeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _opTypeRepositoryMock = new Mock<IFinancialOperationTypeRepository>();
        _opRepositoryMock = new Mock<IFinancialOperationRepository>();
        _createValidatorMock = new Mock<IValidator<OperationTypeCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<OperationTypeUpdateDto>>();
        _loggerMock = new Mock<ILogger<OperationTypeService>>();

        _unitOfWorkMock.Setup(u => u.FinancialOperationTypes).Returns(_opTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FinancialOperations).Returns(_opRepositoryMock.Object);

        _sut = new OperationTypeService(
            _unitOfWorkMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OperationTypeService(null!, _createValidatorMock.Object, _updateValidatorMock.Object, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullCreateValidator_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OperationTypeService(_unitOfWorkMock.Object, null!, _updateValidatorMock.Object, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("createValidator");
    }

    [Fact]
    public void Constructor_WithNullUpdateValidator_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OperationTypeService(_unitOfWorkMock.Object, _createValidatorMock.Object, null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("updateValidator");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OperationTypeService(_unitOfWorkMock.Object, _createValidatorMock.Object, _updateValidatorMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GetTypeByIdAsync Tests

    [Fact]
    public async Task GetTypeByIdAsync_WithExistingId_ReturnsOperationTypeDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "Salary",
            Description = "Monthly salary",
            Kind = OperationKind.Income
        };

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.GetTypeByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be("Salary");
        result.Description.Should().Be("Monthly salary");
        result.Kind.Should().Be(OperationKind.Income);
    }

    [Fact]
    public async Task GetTypeByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperationType?)null);

        // Act
        var act = async () => await _sut.GetTypeByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task GetTypeByIdAsync_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var entity = new FinancialOperationType { Id = id, Name = "Test", Kind = OperationKind.Income };

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, cts.Token))
            .ReturnsAsync(entity);

        // Act
        await _sut.GetTypeByIdAsync(id, cts.Token);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.GetByIdAsync(id, cts.Token), Times.Once);
        cts.Dispose();
    }

    #endregion

    #region GetAllTypesAsync Tests

    [Fact]
    public async Task GetAllTypesAsync_ReturnsAllOperationTypes()
    {
        // Arrange
        var entities = new List<FinancialOperationType>
        {
            new() { Id = Guid.NewGuid(), Name = "Salary", Description = "Income", Kind = OperationKind.Income },
            new() { Id = Guid.NewGuid(), Name = "Rent", Description = "Expense", Kind = OperationKind.Expense }
        };

        _opTypeRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _sut.GetAllTypesAsync();

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(2)
            .And.Contain(r => r.Name == "Salary")
            .And.Contain(r => r.Name == "Rent");
    }

    [Fact]
    public async Task GetAllTypesAsync_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _opTypeRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.GetAllTypesAsync();

        // Assert
        result.Should().NotBeNull()
            .And.BeEmpty();
    }

    [Fact]
    public async Task GetAllTypesAsync_PassesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _opTypeRepositoryMock.Setup(r => r.GetAllAsync(cts.Token))
            .ReturnsAsync([]);

        // Act
        await _sut.GetAllTypesAsync(cts.Token);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
        cts.Dispose();
    }

    #endregion

    #region CreateTypeAsync Tests

    [Fact]
    public async Task CreateTypeAsync_WithValidData_ReturnsNewId()
    {
        // Arrange
        var createDto = new OperationTypeCreateDto("Salary", "Monthly income", OperationKind.Income);

        _createValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeCreateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            null, createDto.Name, createDto.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _opTypeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<FinancialOperationType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateTypeAsync(createDto);

        // Assert
        result.Should().NotBe(Guid.Empty);

        _opTypeRepositoryMock.Verify(r => r.AddAsync(
            It.Is<FinancialOperationType>(e =>
                e.Name == "Salary" &&
                e.Description == "Monthly income" &&
                e.Kind == OperationKind.Income),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTypeAsync_TrimsNameWhitespace()
    {
        // Arrange
        var createDto = new OperationTypeCreateDto("  Salary  ", "Description", OperationKind.Income);

        _createValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeCreateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            null, createDto.Name, createDto.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.CreateTypeAsync(createDto);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.AddAsync(
            It.Is<FinancialOperationType>(e => e.Name == "Salary"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTypeAsync_WithDuplicateNameAndKind_ThrowsConflictException()
    {
        // Arrange
        var createDto = new OperationTypeCreateDto("Salary", "Description", OperationKind.Income);

        _createValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeCreateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            null, createDto.Name, createDto.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.CreateTypeAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");

        _opTypeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialOperationType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTypeAsync_PassesCancellationToken()
    {
        // Arrange
        var createDto = new OperationTypeCreateDto("Salary", "Description", OperationKind.Income);
        var cts = new CancellationTokenSource();

        _createValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeCreateDto>>(),
            cts.Token))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            null, createDto.Name, createDto.Kind, cts.Token))
            .ReturnsAsync(false);

        // Act
        await _sut.CreateTypeAsync(createDto, cts.Token);

        // Assert
        _createValidatorMock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeCreateDto>>(),
            cts.Token), Times.Once);

        _opTypeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialOperationType>(), cts.Token),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
        cts.Dispose();
    }

    #endregion

    #region UpdateTypeAsync Tests

    [Fact]
    public async Task UpdateTypeAsync_WithValidData_UpdatesEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "OldName",
            Description = "Old description",
            Kind = OperationKind.Income
        };
        var updateDto = new OperationTypeUpdateDto("NewName", "New description");

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            id, updateDto.Name, entity.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        entity.Name.Should().Be("NewName");
        entity.Description.Should().Be("New description");

        _opTypeRepositoryMock.Verify(r => r.Update(entity), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTypeAsync_TrimsNameWhitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "OldName",
            Description = "Description",
            Kind = OperationKind.Income
        };
        var updateDto = new OperationTypeUpdateDto("  NewName  ", "Description");

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            id, "NewName", entity.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        entity.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateTypeAsync_WithSameName_DoesNotCheckForDuplicates()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "SameName",
            Description = "Old description",
            Kind = OperationKind.Income
        };
        var updateDto = new OperationTypeUpdateDto("SameName", "New description");

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.ExistsByNameKindAsync(
            It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<OperationKind>(), It.IsAny<CancellationToken>()),
            Times.Never);

        entity.Description.Should().Be("New description");
    }

    [Fact]
    public async Task UpdateTypeAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new OperationTypeUpdateDto("Name", "Description");

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperationType?)null);

        // Act
        var act = async () => await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task UpdateTypeAsync_WithDuplicateNameAndKind_ThrowsConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "OldName",
            Description = "Description",
            Kind = OperationKind.Income
        };
        var updateDto = new OperationTypeUpdateDto("DuplicateName", "Description");

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(
            id, updateDto.Name, entity.Kind, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");

        _opTypeRepositoryMock.Verify(r => r.Update(It.IsAny<FinancialOperationType>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTypeAsync_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType { Id = id, Name = "Name", Kind = OperationKind.Income };
        var updateDto = new OperationTypeUpdateDto("NewName", "Description");
        var cts = new CancellationTokenSource();

        _updateValidatorMock.Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            cts.Token))
            .ReturnsAsync(new ValidationResult());

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, cts.Token))
            .ReturnsAsync(entity);

        _opTypeRepositoryMock.Setup(r => r.ExistsByNameKindAsync(id, "NewName", entity.Kind, cts.Token))
            .ReturnsAsync(false);

        // Act
        await _sut.UpdateTypeAsync(id, updateDto, cts.Token);

        // Assert
        _updateValidatorMock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<OperationTypeUpdateDto>>(),
            cts.Token), Times.Once);

        _opTypeRepositoryMock.Verify(r => r.GetByIdAsync(id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
        cts.Dispose();
    }

    #endregion

    #region DeleteTypeAsync Tests

    [Fact]
    public async Task DeleteTypeAsync_WithExistingId_DeletesEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "ToDelete",
            Description = "Description",
            Kind = OperationKind.Income
        };

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        await _sut.DeleteTypeAsync(id);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.Delete(entity), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTypeAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperationType?)null);

        // Act
        var act = async () => await _sut.DeleteTypeAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");

        _opTypeRepositoryMock.Verify(r => r.Delete(It.IsAny<FinancialOperationType>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTypeAsync_WithIsUsedType_ThrowsConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType
        {
            Id = id,
            Name = "ToDelete",
            Description = "Description",
            Kind = OperationKind.Income
        };

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _opRepositoryMock.Setup(r => r.AnyByTypeIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.DeleteTypeAsync(id);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*is used*");

        _opTypeRepositoryMock.Verify(r => r.Delete(It.IsAny<FinancialOperationType>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTypeAsync_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new FinancialOperationType { Id = id, Name = "Test", Kind = OperationKind.Income };
        var cts = new CancellationTokenSource();

        _opTypeRepositoryMock.Setup(r => r.GetByIdAsync(id, cts.Token))
            .ReturnsAsync(entity);

        // Act
        await _sut.DeleteTypeAsync(id, cts.Token);

        // Assert
        _opTypeRepositoryMock.Verify(r => r.GetByIdAsync(id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
        cts.Dispose();
    }

    #endregion
}