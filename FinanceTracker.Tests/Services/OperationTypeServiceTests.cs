using FinanceTracker.Application.DTOs.OperationType;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
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
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ILogger<OperationTypeService>> _loggerMock;
    private readonly OperationTypeService _sut;

    private readonly Guid _defaultUserId = Guid.NewGuid();

    public OperationTypeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _opTypeRepositoryMock = new Mock<IFinancialOperationTypeRepository>();
        _opRepositoryMock = new Mock<IFinancialOperationRepository>();
        _createValidatorMock = new Mock<IValidator<OperationTypeCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<OperationTypeUpdateDto>>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<OperationTypeService>>();

        _unitOfWorkMock.Setup(u => u.FinancialOperationTypes).Returns(_opTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FinancialOperations).Returns(_opRepositoryMock.Object);

        // Default setup for user context
        SetupUserContext(_defaultUserId);

        _sut = new OperationTypeService(
            _unitOfWorkMock.Object,
            _createValidatorMock.Object,
            _userContextMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private void SetupUserContext(Guid userId)
    {
        _userContextMock.Setup(c => c.GetRequiredUserId()).Returns(userId);
    }

    private FinancialOperationType CreateOperationType(
        Guid? id = null,
        Guid? userId = null,
        string name = "TestType",
        string description = "Test Description",
        OperationKind kind = OperationKind.Income)
    {
        return new FinancialOperationType
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? _defaultUserId,
            Name = name,
            Description = description,
            Kind = kind
        };
    }

    private void SetupValidValidation<T>()
    {
        if (typeof(T) == typeof(OperationTypeCreateDto))
        {
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<OperationTypeCreateDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
        else if (typeof(T) == typeof(OperationTypeUpdateDto))
        {
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<OperationTypeUpdateDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
    }

    private void SetupGetByIdForUser(Guid id, FinancialOperationType? entity)
    {
        _opTypeRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    private void SetupExistsByNameKind(string name, OperationKind kind, Guid? excludeId, bool exists)
    {
        _opTypeRepositoryMock
            .Setup(r => r.ExistsByNameKindAsync(_defaultUserId, name, kind, excludeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupSuccessfulSave()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion

    #region GetTypeByIdAsync Tests

    [Fact]
    public async Task GetTypeByIdAsync_WithExistingId_ReturnsOperationTypeDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateOperationType(id: id, name: "Salary", description: "Monthly salary");
        SetupGetByIdForUser(id, entity);

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
        SetupGetByIdForUser(id, null);

        // Act
        var act = async () => await _sut.GetTypeByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }

    #endregion

    #region GetUserTypesAsync Tests

    [Fact]
    public async Task GetUserTypesAsync_ReturnsAllOperationTypes()
    {
        // Arrange
        var entities = new List<FinancialOperationType>
        {
            CreateOperationType(name: "Salary", description: "Income", kind: OperationKind.Income),
            CreateOperationType(name: "Rent", description: "Expense", kind: OperationKind.Expense)
        };

        _opTypeRepositoryMock
            .Setup(r => r.GetUserTypesAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _sut.GetUserTypesAsync();

        // Assert
        result.Should().HaveCount(2)
            .And.Contain(r => r.Name == "Salary")
            .And.Contain(r => r.Name == "Rent");
    }

    [Fact]
    public async Task GetUserTypesAsync_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _opTypeRepositoryMock
            .Setup(r => r.GetUserTypesAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FinancialOperationType>());

        // Act
        var result = await _sut.GetUserTypesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateTypeAsync Tests

    [Fact]
    public async Task CreateTypeAsync_WithValidData_ReturnsNewId()
    {
        // Arrange
        var createDto = new OperationTypeCreateDto
        {
            Name = "Salary",
            Description = "Monthly income",
            Kind = OperationKind.Income
        };

        SetupValidValidation<OperationTypeCreateDto>();
        SetupExistsByNameKind(createDto.Name, createDto.Kind, null, false);
        SetupSuccessfulSave();

        // Act
        var result = await _sut.CreateTypeAsync(createDto);

        // Assert
        result.Should().NotBe(Guid.Empty);

        _opTypeRepositoryMock.Verify(r => r.AddAsync(
            It.Is<FinancialOperationType>(e =>
                e.UserId == _defaultUserId &&
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
        var createDto = new OperationTypeCreateDto
        {
            Name = "  Salary  ",
            Description = "Monthly income",
            Kind = OperationKind.Income
        };

        SetupValidValidation<OperationTypeCreateDto>();
        SetupExistsByNameKind("  Salary  ", createDto.Kind, null, false);
        SetupSuccessfulSave();

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
        var createDto = new OperationTypeCreateDto
        {
            Name = "Salary",
            Description = "Monthly income",
            Kind = OperationKind.Income
        };

        SetupValidValidation<OperationTypeCreateDto>();
        SetupExistsByNameKind(createDto.Name, createDto.Kind, null, true);

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
    public async Task CreateTypeAsync_WhenUserContextThrows_PropagatesException()
    {
        // Arrange
        _userContextMock.Setup(c => c.GetRequiredUserId())
            .Throws(new UnauthorizedAccessException("User not authenticated"));

        // Act
        var act = async () => await _sut.CreateTypeAsync(new OperationTypeCreateDto());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region UpdateTypeAsync Tests

    [Fact]
    public async Task UpdateTypeAsync_WithValidData_UpdatesEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateOperationType(id: id, name: "OldName", description: "Old description");
        var updateDto = new OperationTypeUpdateDto
        {
            Name = "NewName",
            Description = "New description"
        };

        SetupValidValidation<OperationTypeUpdateDto>();
        SetupGetByIdForUser(id, entity);
        SetupExistsByNameKind(updateDto.Name, entity.Kind, id, false);
        SetupSuccessfulSave();

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
        var entity = CreateOperationType(id: id, name: "OldName");
        var updateDto = new OperationTypeUpdateDto
        {
            Name = "  NewName  ",
            Description = "New description"
        };

        SetupValidValidation<OperationTypeUpdateDto>();
        SetupGetByIdForUser(id, entity);
        SetupExistsByNameKind("NewName", entity.Kind, id, false);
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        entity.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateTypeAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new OperationTypeUpdateDto
        {
            Name = "NewName",
            Description = "New description"
        };

        SetupValidValidation<OperationTypeUpdateDto>();
        SetupGetByIdForUser(id, null);

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
        var entity = CreateOperationType(id: id, name: "OldName");
        var updateDto = new OperationTypeUpdateDto
        {
            Name = "NewName",
            Description = "New description"
        };

        SetupValidValidation<OperationTypeUpdateDto>();
        SetupGetByIdForUser(id, entity);
        SetupExistsByNameKind(updateDto.Name, entity.Kind, id, true);

        // Act
        var act = async () => await _sut.UpdateTypeAsync(id, updateDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");

        _opTypeRepositoryMock.Verify(r => r.Update(It.IsAny<FinancialOperationType>()), Times.Never);
    }

    #endregion

    #region DeleteTypeAsync Tests

    [Fact]
    public async Task DeleteTypeAsync_WithExistingId_DeletesEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateOperationType(id: id, name: "ToDelete");

        SetupGetByIdForUser(id, entity);
        _opRepositoryMock
            .Setup(r => r.AnyByTypeIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        SetupSuccessfulSave();

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
        SetupGetByIdForUser(id, null);

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
        var entity = CreateOperationType(id: id, name: "ToDelete");

        SetupGetByIdForUser(id, entity);
        _opRepositoryMock
            .Setup(r => r.AnyByTypeIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.DeleteTypeAsync(id);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*is used*");

        _opTypeRepositoryMock.Verify(r => r.Delete(It.IsAny<FinancialOperationType>()), Times.Never);
    }

    #endregion
}