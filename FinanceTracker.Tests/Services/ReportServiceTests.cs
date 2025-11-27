using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceTracker.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFinancialOperationRepository> _opRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ILogger<ReportService>> _loggerMock;
    private readonly ReportService _sut;

    private readonly Guid _defaultUserId = Guid.NewGuid();
    private readonly Guid _defaultWalletId = Guid.NewGuid();

    public ReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _opRepositoryMock = new Mock<IFinancialOperationRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<ReportService>>();

        _unitOfWorkMock.Setup(u => u.FinancialOperations).Returns(_opRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Wallets).Returns(_walletRepositoryMock.Object);

        SetupUserContext(_defaultUserId);

        _sut = new ReportService(_unitOfWorkMock.Object, _userContextMock.Object, _loggerMock.Object);
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
            BaseCurrencyCode = currencyCode,
            IsDeleted = false
        };
    }

    private FinancialOperation CreateOperation(
        Guid? id = null,
        Guid? walletId = null,
        Guid? typeId = null,
        string typeName = "Food",
        OperationKind kind = OperationKind.Expense,
        decimal amountBase = 100m,
        decimal? amountOriginal = null,
        string? currencyCode = null,
        DateTime? date = null,
        string? note = null)
    {
        var opId = id ?? Guid.NewGuid();
        var opWalletId = walletId ?? _defaultWalletId;
        var opTypeId = typeId ?? Guid.NewGuid();

        return new FinancialOperation
        {
            Id = opId,
            WalletId = opWalletId,
            TypeId = opTypeId,
            Type = new FinancialOperationType
            {
                Id = opTypeId,
                Name = typeName,
                Kind = kind
            },
            Wallet = new Wallet
            {
                Id = opWalletId,
                Name = "My Wallet",
                BaseCurrencyCode = "USD"
            },
            AmountBase = amountBase,
            AmountOriginal = amountOriginal ?? amountBase,
            CurrencyOriginalCode = currencyCode,
            Date = date ?? DateTime.UtcNow,
            Note = note
        };
    }

    private void SetupValidWallet(Guid walletId, Wallet? wallet = null)
    {
        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet ?? CreateWallet(id: walletId));
    }

    private void SetupOperationsForDate(Guid walletId, DateOnly date, List<FinancialOperation> operations)
    {
        _opRepositoryMock
            .Setup(r => r.GetListByDateAsync(walletId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);
    }

    private void SetupOperationsForPeriod(Guid walletId, DateOnly start, DateOnly end, List<FinancialOperation> operations)
    {
        _opRepositoryMock
            .Setup(r => r.GetListByPeriodAsync(walletId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);
    }

    #endregion

    #region CreateDailyReportAsync Tests

    [Fact]
    public async Task CreateDailyReportAsync_WithValidData_ReturnsCorrectReport()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);
        var dateTime = new DateTime(2025, 11, 12);

        var wallet = CreateWallet(id: walletId, name: "Main Wallet", currencyCode: "USD");

        var salaryTypeId = Guid.NewGuid();
        var freelanceTypeId = Guid.NewGuid();
        var foodTypeId = Guid.NewGuid();
        var rentTypeId = Guid.NewGuid();

        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, typeId: salaryTypeId, typeName: "Salary",
                kind: OperationKind.Income, amountBase: 1000m, date: dateTime),
            CreateOperation(walletId: walletId, typeId: freelanceTypeId, typeName: "Freelance",
                kind: OperationKind.Income, amountBase: 500m, date: dateTime),
            CreateOperation(walletId: walletId, typeId: foodTypeId, typeName: "Food",
                kind: OperationKind.Expense, amountBase: 150.10m, date: dateTime),
            CreateOperation(walletId: walletId, typeId: foodTypeId, typeName: "Food",
                kind: OperationKind.Expense, amountBase: 49.90m, date: dateTime),
            CreateOperation(walletId: walletId, typeId: rentTypeId, typeName: "Rent",
                kind: OperationKind.Expense, amountBase: 400m, date: dateTime)
        };

        SetupValidWallet(walletId, wallet);
        SetupOperationsForDate(walletId, date, operations);

        // Act
        var result = await _sut.CreateDailyReportAsync(walletId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1500m);
        result.TotalExpense.Should().Be(600m); // 150.10 + 49.90 + 400
        result.Net.Should().Be(900m);

        result.IncomeByCategory.Should().HaveCount(2);
        result.ExpensesByCategory.Should().HaveCount(2);

        var salaryCategory = result.IncomeByCategory.Single(c => c.TypeName == "Salary");
        salaryCategory.Amount.Should().Be(1000m);
        salaryCategory.TypeId.Should().Be(salaryTypeId);

        var freelanceCategory = result.IncomeByCategory.Single(c => c.TypeName == "Freelance");
        freelanceCategory.Amount.Should().Be(500m);
        freelanceCategory.TypeId.Should().Be(freelanceTypeId);

        var foodCategory = result.ExpensesByCategory.Single(c => c.TypeName == "Food");
        foodCategory.Amount.Should().Be(200m); // 150.10 + 49.90

        var rentCategory = result.ExpensesByCategory.Single(c => c.TypeName == "Rent");
        rentCategory.Amount.Should().Be(400m);
    }

    [Fact]
    public async Task CreateDailyReportAsync_WithNoOperations_ReturnsEmptyReport()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);

        SetupValidWallet(walletId);
        SetupOperationsForDate(walletId, date, new List<FinancialOperation>());

        // Act
        var result = await _sut.CreateDailyReportAsync(walletId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
        result.Net.Should().Be(0m);
        result.IncomeByCategory.Should().BeEmpty();
        result.ExpensesByCategory.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDailyReportAsync_WithNonExistentWallet_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);

        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.CreateDailyReportAsync(walletId, date);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");
    }

    [Fact]
    public async Task CreateDailyReportAsync_WithMultiCurrencyOperations_CalculatesCorrectly()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);
        var dateTime = new DateTime(2025, 11, 12);

        var operations = new List<FinancialOperation>
        {
            // Operation in base currency
            CreateOperation(walletId: walletId, typeName: "Salary", kind: OperationKind.Income,
                amountBase: 1000m, amountOriginal: 1000m, currencyCode: null, date: dateTime),
            
            // Operation in EUR converted to USD
            CreateOperation(walletId: walletId, typeName: "Shopping", kind: OperationKind.Expense,
                amountBase: 108m, amountOriginal: 100m, currencyCode: "EUR", date: dateTime)
        };

        SetupValidWallet(walletId);
        SetupOperationsForDate(walletId, date, operations);

        // Act
        var result = await _sut.CreateDailyReportAsync(walletId, date);

        // Assert
        result.TotalIncome.Should().Be(1000m);
        result.TotalExpense.Should().Be(108m);
        result.Net.Should().Be(892m);

        result.IncomeByCategory.Should().ContainSingle(c => c.TypeName == "Salary" && c.Amount == 1000m);
        result.ExpensesByCategory.Should().ContainSingle(c => c.TypeName == "Shopping" && c.Amount == 108m);
    }

    [Fact]
    public async Task CreateDailyReportAsync_WhenUserContextThrows_PropagatesException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);

        _userContextMock.Setup(c => c.GetRequiredUserId())
            .Throws(new UnauthorizedAccessException("User not authenticated"));

        // Act
        var act = async () => await _sut.CreateDailyReportAsync(walletId, date);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region CreatePeriodReportAsync Tests

    [Fact]
    public async Task CreatePeriodReportAsync_WithValidRange_ReturnsCorrectReport()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 10);
        var end = new DateOnly(2025, 11, 12);
        var travelTypeId = Guid.NewGuid();

        var wallet = CreateWallet(id: walletId, name: "Main Wallet", currencyCode: "EUR");
        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, typeName: "Salary", kind: OperationKind.Income,
                amountBase: 1000m, date: new DateTime(2025, 11, 10)),
            CreateOperation(walletId: walletId, typeId: travelTypeId, typeName: "Travel", kind: OperationKind.Expense,
                amountBase: 300m, date: new DateTime(2025, 11, 11)),
            CreateOperation(walletId: walletId, typeId: travelTypeId, typeName: "Travel", kind: OperationKind.Expense,
                amountBase: 200m, date: new DateTime(2025, 11, 12))
        };

        SetupValidWallet(walletId, wallet);
        SetupOperationsForPeriod(walletId, start, end, operations);

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1000m);
        result.TotalExpense.Should().Be(500m);
        result.Net.Should().Be(500m);

        result.IncomeByCategory.Should().ContainSingle(c => c.TypeName == "Salary" && c.Amount == 1000m);
        result.ExpensesByCategory.Should().ContainSingle(c => c.TypeName == "Travel" && c.Amount == 500m);
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithSingleDayRange_ReturnsCorrectReport()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var date = new DateOnly(2025, 11, 12);

        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, typeName: "Lunch", kind: OperationKind.Expense,
                amountBase: 50m, date: new DateTime(2025, 11, 12))
        };

        SetupValidWallet(walletId);
        SetupOperationsForPeriod(walletId, date, date, operations);

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, date, date);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(50m);
        result.Net.Should().Be(-50m);
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithNoOperations_ReturnsEmptyReport()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 01);
        var end = new DateOnly(2025, 11, 30);

        SetupValidWallet(walletId);
        SetupOperationsForPeriod(walletId, start, end, new List<FinancialOperation>());

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
        result.Net.Should().Be(0m);
        result.IncomeByCategory.Should().BeEmpty();
        result.ExpensesByCategory.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithStartAfterEnd_ThrowsValidationException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 13);
        var end = new DateOnly(2025, 11, 12);

        // Act
        var act = async () => await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*End date must be after or equal to start date*");

        _opRepositoryMock.Verify(
            r => r.GetListByPeriodAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithNonExistentWallet_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 10);
        var end = new DateOnly(2025, 11, 12);

        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithOnlyIncomeOperations_CalculatesCorrectly()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 01);
        var end = new DateOnly(2025, 11, 30);

        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, typeName: "Salary", kind: OperationKind.Income,
                amountBase: 5000m, date: new DateTime(2025, 11, 1)),
            CreateOperation(walletId: walletId, typeName: "Freelance", kind: OperationKind.Income,
                amountBase: 1500m, date: new DateTime(2025, 11, 15))
        };

        SetupValidWallet(walletId);
        SetupOperationsForPeriod(walletId, start, end, operations);

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        result.TotalIncome.Should().Be(6500m);
        result.TotalExpense.Should().Be(0m);
        result.Net.Should().Be(6500m);

        result.IncomeByCategory.Should().HaveCount(2);
        result.ExpensesByCategory.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithOnlyExpenseOperations_CalculatesCorrectly()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 11, 01);
        var end = new DateOnly(2025, 11, 30);

        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, typeName: "Rent", kind: OperationKind.Expense,
                amountBase: 1000m, date: new DateTime(2025, 11, 1)),
            CreateOperation(walletId: walletId, typeName: "Food", kind: OperationKind.Expense,
                amountBase: 500m, date: new DateTime(2025, 11, 15))
        };

        SetupValidWallet(walletId);
        SetupOperationsForPeriod(walletId, start, end, operations);

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(1500m);
        result.Net.Should().Be(-1500m);
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithLargeDateRange_HandlesCorrectly()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var start = new DateOnly(2025, 01, 01);
        var end = new DateOnly(2025, 12, 31);

        var operations = new List<FinancialOperation>
        {
            CreateOperation(walletId: walletId, amountBase: 1000m,
                date: new DateTime(2025, 1, 1)),
            CreateOperation(walletId: walletId, amountBase: 2000m,
                date: new DateTime(2025, 12, 31))
        };

        SetupValidWallet(walletId);
        SetupOperationsForPeriod(walletId, start, end, operations);

        // Act
        var result = await _sut.CreatePeriodReportAsync(walletId, start, end);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(3000m);
        result.Net.Should().Be(-3000m);
    }

    #endregion
}