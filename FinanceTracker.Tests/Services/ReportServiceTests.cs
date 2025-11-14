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
    private readonly Mock<ILogger<ReportService>> _loggerMock;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _opRepositoryMock = new Mock<IFinancialOperationRepository>();
        _loggerMock = new Mock<ILogger<ReportService>>();

        _unitOfWorkMock.Setup(u => u.FinancialOperations).Returns(_opRepositoryMock.Object);

        _sut = new ReportService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    private static FinancialOperation CreateOperation(Guid? id, Guid typeId, string typeName,
        OperationKind kind, decimal amount, DateTime date, string? note = null)
    {
        return new FinancialOperation
        {
            Id = id ?? Guid.NewGuid(),
            TypeId = typeId,
            Type = new FinancialOperationType
            {
                Id = typeId,
                Name = typeName,
                Kind = kind
            },
            AmountBase = amount,
            Date = date,
            Note = note
        };
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReportService(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReportService(_unitOfWorkMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task CreateDailyReportAsync_WithValidData_ReturnsCorrectTotalsAndOperations()
    {
        // Arrange
        var date = new DateTime(2025, 11, 12);
        var dateOnly = new DateOnly(2025, 11, 12);
        var foodId = Guid.NewGuid();
        var salaryId = Guid.NewGuid();

        var data = new List<FinancialOperation>
        {
            CreateOperation(null, salaryId, "Salary", OperationKind.Income, 1200.50m, date ),
            CreateOperation(null, foodId, "Food", OperationKind.Expense, 150.10m, date),
            CreateOperation(null, foodId, "Food", OperationKind.Expense, 49.90m, date, "Snacks")
        };

        _opRepositoryMock
            .Setup(r => r.GetListByDateAsync(dateOnly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        // Act
        var result = await _sut.CreateDailyReportAsync(dateOnly, CancellationToken.None);

        // Assert
        result.Start.Should().Be(dateOnly);
        result.End.Should().Be(dateOnly);
        result.TotalIncome.Should().Be(1200.50m);
        result.TotalExpense.Should().Be(200.00m); // 150.10 + 49.90
        result.Operations.Should().HaveCount(3);

        // Check mapping of one item
        var first = result.Operations.First(o => o.TypeName == "Salary");
        first.Kind.Should().Be(OperationKind.Income);
        first.Amount.Should().Be(1200.50m);
        first.Date.Should().Be(date);

        _opRepositoryMock.Verify(r => r.GetListByDateAsync(dateOnly, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyGet(u => u.FinancialOperations, Times.AtLeastOnce);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateDailyReportAsync_WithNoOperations_ReturnsEmptyListAndZeroTotals()
    {
        // Arrange
        var start = new DateOnly(2025, 11, 10);
        var end = new DateOnly(2025, 11, 12);

        var travelId = Guid.NewGuid();
        var salaryId = Guid.NewGuid();

        var data = new List<FinancialOperation>
        {
            CreateOperation(null, salaryId, "Salary", OperationKind.Income, 1000m, new DateTime(2025, 11, 10, 15, 15, 15)),
            CreateOperation(null, travelId, "Travel", OperationKind.Expense, 300m, new DateTime(2025, 11, 11, 15, 15, 15)),
            CreateOperation(null, travelId, "Travel", OperationKind.Expense, 200m, new DateTime(2025, 11, 12, 15, 15, 15))
        };

        _opRepositoryMock
            .Setup(r => r.GetListByPeriodAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        // Act
        var result = await _sut.CreatePeriodReportAsync(start, end, CancellationToken.None);

        // Assert
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.TotalIncome.Should().Be(1000m);
        result.TotalExpense.Should().Be(500m);
        result.Operations.Should().HaveCount(3);

        // ensure repository method used matches the period overload
        _opRepositoryMock.Verify(r => r.GetListByPeriodAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyGet(u => u.FinancialOperations, Times.AtLeastOnce);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithValidRange_ReturnsCorrectTotalsAndOperations()
    {
        // Arrange
        var start = new DateOnly(2025, 11, 13);
        var end = new DateOnly(2025, 11, 12);

        // Act
        var act = async () => await _sut.CreatePeriodReportAsync(start, end, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
                 .WithMessage("*End date must be after or equal to start date*");

        _opRepositoryMock.Verify(r => r.GetListByPeriodAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _opRepositoryMock.Verify(r => r.GetListByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithNoOperations_ReturnsEmptyListAndZeroTotals()
    {
        // Arrange
        var date = new DateOnly(2025, 11, 12);

        _opRepositoryMock
            .Setup(r => r.GetListByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.CreateDailyReportAsync(date, CancellationToken.None);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
        result.Operations.Should().BeEmpty();

        _opRepositoryMock.Verify(r => r.GetListByDateAsync(date, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreatePeriodReportAsync_WithStartAfterEnd_ThrowsValidationException()
    {
        // Arrange
        var start = new DateOnly(2025, 11, 01);
        var end = new DateOnly(2025, 11, 30);

        _opRepositoryMock
            .Setup(r => r.GetListByPeriodAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.CreatePeriodReportAsync(start, end, CancellationToken.None);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
        result.Operations.Should().BeEmpty();

        _opRepositoryMock.Verify(r => r.GetListByPeriodAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }
}
