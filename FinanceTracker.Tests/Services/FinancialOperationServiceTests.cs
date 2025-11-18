using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceTracker.Tests.Services;

public class FinancialOperationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFinancialOperationRepository> _opRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IFinancialOperationTypeRepository> _opTypeRepositoryMock;
    private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IValidator<FinancialOperationUpsertDto>> _upsertValidatorMock;
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
    private readonly Mock<ILogger<FinancialOperationService>> _loggerMock;
    private readonly FinancialOperationService _sut;

    private readonly Guid _defaultUserId = Guid.NewGuid();
    private readonly Guid _defaultWalletId = Guid.NewGuid();
    private readonly Guid _defaultTypeId = Guid.NewGuid();

    public FinancialOperationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _opRepositoryMock = new Mock<IFinancialOperationRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _opTypeRepositoryMock = new Mock<IFinancialOperationTypeRepository>();
        _currencyRepositoryMock = new Mock<ICurrencyRepository>();
        _userContextMock = new Mock<IUserContext>();
        _upsertValidatorMock = new Mock<IValidator<FinancialOperationUpsertDto>>();
        _exchangeRateServiceMock = new Mock<IExchangeRateService>();
        _loggerMock = new Mock<ILogger<FinancialOperationService>>();

        _unitOfWorkMock.Setup(u => u.FinancialOperations).Returns(_opRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Wallets).Returns(_walletRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FinancialOperationTypes).Returns(_opTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Currencies).Returns(_currencyRepositoryMock.Object);

        SetupUserContext(_defaultUserId);

        _sut = new FinancialOperationService(
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _upsertValidatorMock.Object,
            _exchangeRateServiceMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private void SetupUserContext(Guid userId)
    {
        _userContextMock.Setup(c => c.GetRequiredUserId()).Returns(userId);
    }

    private Wallet CreateWallet(
        Guid? id = null,
        Guid? userId = null,
        string name = "My Wallet",
        string currencyCode = "USD")
    {
        return new Wallet
        {
            Id = id ?? _defaultWalletId,
            UserId = userId ?? _defaultUserId,
            Name = name,
            BaseCurrencyCode = currencyCode
        };
    }

    private FinancialOperationType CreateOperationType(
        Guid? id = null,
        Guid? userId = null,
        string name = "Food",
        OperationKind kind = OperationKind.Expense)
    {
        return new FinancialOperationType
        {
            Id = id ?? _defaultTypeId,
            UserId = userId ?? _defaultUserId,
            Name = name,
            Kind = kind
        };
    }

    private FinancialOperation CreateOperation(
        Guid? id = null,
        Guid? walletId = null,
        Guid? typeId = null,
        decimal amountBase = 100m,
        decimal amountOriginal = 100m,
        string? currencyCode = null,
        DateTime? date = null,
        string? note = null)
    {
        var opId = id ?? Guid.NewGuid();
        var opWalletId = walletId ?? _defaultWalletId;
        var opTypeId = typeId ?? _defaultTypeId;

        return new FinancialOperation
        {
            Id = opId,
            WalletId = opWalletId,
            TypeId = opTypeId,
            Type = CreateOperationType(id: opTypeId),
            Wallet = CreateWallet(id: opWalletId),
            AmountBase = amountBase,
            AmountOriginal = amountOriginal,
            CurrencyOriginalCode = currencyCode,
            Date = date ?? DateTime.UtcNow,
            Note = note
        };
    }

    private FinancialOperationUpsertDto CreateUpsertDto(
        Guid? typeId = null,
        decimal amountOriginal = 100m,
        string? currencyCode = "EUR",
        DateTime? date = null,
        string? note = null)
    {
        return new FinancialOperationUpsertDto
        {
            TypeId = typeId ?? _defaultTypeId,
            AmountOriginal = amountOriginal,
            CurrencyOriginalCode = currencyCode,
            Date = date ?? DateTime.UtcNow,
            Note = note
        };
    }

    private void SetupValidValidation()
    {
        _upsertValidatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<FinancialOperationUpsertDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidWallet(Guid walletId, Wallet? wallet = null)
    {
        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet ?? CreateWallet(id: walletId));
    }

    private void SetupValidOperationType(Guid typeId, FinancialOperationType? type = null)
    {
        _opTypeRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type ?? CreateOperationType(id: typeId));
    }

    private void SetupCurrencyExists(string currencyCode, bool exists = true)
    {
        _currencyRepositoryMock
            .Setup(r => r.ExistsAsync(currencyCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupExchangeRate(string from, string to, DateTime date, decimal rate)
    {
        _exchangeRateServiceMock
            .Setup(s => s.GetExchangeRateAsync(from, to, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);
    }

    private void SetupSuccessfulSave()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion

    #region GetOperationByIdAsync Tests

    [Fact]
    public async Task GetOperationByIdAsync_WithExistingId_ReturnsOperationDto()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var operation = CreateOperation(
            id: operationId,
            walletId: walletId,
            amountBase: 108m,
            amountOriginal: 100m,
            currencyCode: "EUR",
            note: "Test note");

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var result = await _sut.GetOperationByIdAsync(walletId, operationId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(operationId);
        result.WalletId.Should().Be(walletId);
        result.AmountBase.Should().Be(108m);
        result.AmountOriginal.Should().Be(100m);
        result.CurrencyOriginalCode.Should().Be("EUR");
        result.Note.Should().Be("Test note");
    }

    [Fact]
    public async Task GetOperationByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperation?)null);

        // Act
        var act = async () => await _sut.GetOperationByIdAsync(walletId, operationId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{operationId}*");
    }

    #endregion

    #region GetAllOperationsAsync Tests

    [Fact]
    public async Task GetAllOperationsAsync_ReturnsAllOperations()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, amountBase: 100m),
            CreateOperation(walletId: walletId, amountBase: 200m)
        };

        _opRepositoryMock
            .Setup(r => r.GetWalletOperationsAsync(walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);

        // Act
        var result = await _sut.GetAllOperationsAsync(walletId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.AmountBase == 100m);
        result.Should().Contain(o => o.AmountBase == 200m);
    }

    [Fact]
    public async Task GetAllOperationsAsync_WithEmptyWallet_ReturnsEmptyList()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        _opRepositoryMock
            .Setup(r => r.GetWalletOperationsAsync(walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FinancialOperation>());

        // Act
        var result = await _sut.GetAllOperationsAsync(walletId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateOperationAsync Tests

    [Fact]
    public async Task CreateOperationAsync_WithValidData_ReturnsNewId()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var createDto = CreateUpsertDto(
            amountOriginal: 100m,
            currencyCode: "EUR",
            note: "Test purchase");

        SetupValidValidation();
        SetupValidWallet(walletId, CreateWallet(id: walletId, currencyCode: "USD"));
        SetupValidOperationType(createDto.TypeId);
        SetupCurrencyExists("EUR");
        SetupExchangeRate("EUR", "USD", createDto.Date, 1.08m);
        SetupSuccessfulSave();

        // Act
        var result = await _sut.CreateOperationAsync(walletId, createDto);

        // Assert
        result.Should().NotBe(Guid.Empty);

        _opRepositoryMock.Verify(r => r.AddAsync(
            It.Is<FinancialOperation>(op =>
                op.WalletId == walletId &&
                op.TypeId == createDto.TypeId &&
                op.AmountOriginal == 100m &&
                op.AmountBase == 108m && // 100 * 1.08
                op.CurrencyOriginalCode == "EUR" &&
                op.Note!.Contains("Original amount: 100 EUR")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOperationAsync_WithoutUserNote_CreatesNoteWithExchangeRateOnly()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var createDto = CreateUpsertDto(
            amountOriginal: 100m,
            currencyCode: "EUR",
            note: null);

        SetupValidValidation();
        SetupValidWallet(walletId);
        SetupValidOperationType(createDto.TypeId);
        SetupCurrencyExists("EUR");
        SetupExchangeRate("EUR", "USD", createDto.Date, 1.08m);
        SetupSuccessfulSave();

        // Act
        await _sut.CreateOperationAsync(walletId, createDto);

        // Assert
        _opRepositoryMock.Verify(r => r.AddAsync(
            It.Is<FinancialOperation>(op =>
                op.Note!.StartsWith("Original amount:") &&
                !op.Note.Contains("\n")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOperationAsync_WithNonExistentWallet_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var createDto = CreateUpsertDto();

        SetupValidValidation();
        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.CreateOperationAsync(walletId, createDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");
    }

    [Fact]
    public async Task CreateOperationAsync_WithNonExistentOperationType_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var createDto = CreateUpsertDto(typeId: typeId);

        SetupValidValidation();
        SetupValidWallet(walletId);
        _opTypeRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperationType?)null);

        // Act
        var act = async () => await _sut.CreateOperationAsync(walletId, createDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{typeId}*");
    }

    [Fact]
    public async Task CreateOperationAsync_WithUnsupportedCurrency_ThrowsValidationException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var createDto = CreateUpsertDto(currencyCode: "XYZ");

        SetupValidValidation();
        SetupValidWallet(walletId);
        SetupValidOperationType(createDto.TypeId);
        SetupCurrencyExists("XYZ", exists: false);

        // Act
        var act = async () => await _sut.CreateOperationAsync(walletId, createDto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*XYZ*not supported*");
    }

    #endregion

    #region UpdateOperationAsync Tests

    [Fact]
    public async Task UpdateOperationAsync_WithChangedAmount_RecalculatesExchangeRate()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var existingOp = CreateOperation(
            id: operationId,
            walletId: walletId,
            amountBase: 108m,
            amountOriginal: 100m,
            currencyCode: "EUR",
            date: new DateTime(2025, 1, 1));

        var updateDto = CreateUpsertDto(
            amountOriginal: 150m, // Changed
            currencyCode: "EUR",
            date: new DateTime(2025, 1, 1));

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOp);

        SetupValidValidation();
        SetupValidOperationType(updateDto.TypeId);
        SetupCurrencyExists("EUR");
        SetupExchangeRate("EUR", "USD", updateDto.Date, 1.08m);
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateOperationAsync(walletId, operationId, updateDto);

        // Assert
        existingOp.AmountOriginal.Should().Be(150m);
        existingOp.AmountBase.Should().Be(162m); // 150 * 1.08

        _opRepositoryMock.Verify(r => r.Update(existingOp), Times.Once);
    }

    [Fact]
    public async Task UpdateOperationAsync_WithChangedCurrency_RecalculatesExchangeRate()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var existingOp = CreateOperation(
            id: operationId,
            walletId: walletId,
            amountOriginal: 100m,
            currencyCode: "EUR");

        var updateDto = CreateUpsertDto(
            amountOriginal: 100m,
            currencyCode: "GBP", // Changed
            date: existingOp.Date);

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOp);

        SetupValidValidation();
        SetupValidOperationType(updateDto.TypeId);
        SetupCurrencyExists("GBP");
        SetupExchangeRate("GBP", "USD", updateDto.Date, 1.25m);
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateOperationAsync(walletId, operationId, updateDto);

        // Assert
        existingOp.CurrencyOriginalCode.Should().Be("GBP");
        existingOp.AmountBase.Should().Be(125m); // 100 * 1.25
    }

    [Fact]
    public async Task UpdateOperationAsync_WithoutRateChangingFields_DoesNotRecalculate()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var newTypeId = Guid.NewGuid();
        var existingOp = CreateOperation(
            id: operationId,
            walletId: walletId,
            amountBase: 108m,
            amountOriginal: 100m,
            currencyCode: "EUR",
            date: new DateTime(2025, 1, 1));

        var updateDto = CreateUpsertDto(
            typeId: newTypeId, // Only type changed
            amountOriginal: 100m,
            currencyCode: "EUR",
            date: new DateTime(2025, 1, 1),
            note: "Updated note");

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOp);

        SetupValidValidation();
        SetupValidOperationType(newTypeId);
        SetupCurrencyExists("EUR");
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateOperationAsync(walletId, operationId, updateDto);

        // Assert
        existingOp.TypeId.Should().Be(newTypeId);
        existingOp.AmountBase.Should().Be(108m); // Not recalculated
        existingOp.Note.Should().Be("Updated note");

        _exchangeRateServiceMock.Verify(
            s => s.GetExchangeRateAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOperationAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var updateDto = CreateUpsertDto();

        _opRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperation?)null);

        // Act
        var act = async () => await _sut.UpdateOperationAsync(walletId, operationId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{operationId}*");
    }

    #endregion

    #region SoftDeleteOperationAsync Tests

    [Fact]
    public async Task SoftDeleteOperationAsync_WithExistingId_DeletesOperation()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var operation = CreateOperation(id: operationId, walletId: walletId);

        _opRepositoryMock
            .Setup(r => r.GetByIdAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        SetupSuccessfulSave();

        // Act
        await _sut.SoftDeleteOperationAsync(walletId, operationId);

        // Assert
        _opRepositoryMock.Verify(r => r.SoftDelete(operation), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteOperationAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var operationId = Guid.NewGuid();

        _opRepositoryMock
            .Setup(r => r.GetByIdAsync(walletId, operationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOperation?)null);

        // Act
        var act = async () => await _sut.SoftDeleteOperationAsync(walletId, operationId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{operationId}*");

        _opRepositoryMock.Verify(
            r => r.SoftDelete(It.IsAny<FinancialOperation>()),
            Times.Never);
    }

    #endregion
}