using FinanceTracker.Application.DTOs.Wallet;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceTracker.Tests.Services;

public class WalletServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IValidator<WalletCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<WalletUpdateDto>> _updateValidatorMock;
    private readonly Mock<ILogger<WalletService>> _loggerMock;
    private readonly WalletService _sut;

    private readonly Guid _defaultUserId = Guid.NewGuid();

    public WalletServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _currencyRepositoryMock = new Mock<ICurrencyRepository>();
        _userContextMock = new Mock<IUserContext>();
        _createValidatorMock = new Mock<IValidator<WalletCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<WalletUpdateDto>>();
        _loggerMock = new Mock<ILogger<WalletService>>();

        _unitOfWorkMock.Setup(u => u.Wallets).Returns(_walletRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Currencies).Returns(_currencyRepositoryMock.Object);

        SetupUserContext(_defaultUserId);

        _sut = new WalletService(
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
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
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? _defaultUserId,
            Name = name,
            BaseCurrencyCode = currencyCode,
            IsDeleted = false
        };
    }

    private void SetupValidValidation<T>()
    {
        if (typeof(T) == typeof(WalletCreateDto))
        {
            _createValidatorMock
                .Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<WalletCreateDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
        else if (typeof(T) == typeof(WalletUpdateDto))
        {
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<WalletUpdateDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
    }

    private void SetupValidWallet(Guid walletId, Wallet? wallet = null)
    {
        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet ?? CreateWallet(id: walletId));
    }

    private void SetupWalletNameExists(string name, Guid? excludeId, bool exists)
    {
        _walletRepositoryMock
            .Setup(r => r.ExistsByNameAsync(_defaultUserId, name, excludeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupCurrencyExists(string currencyCode, bool exists = true)
    {
        _currencyRepositoryMock
            .Setup(r => r.ExistsAsync(currencyCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupSuccessfulSave()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion

    #region GetWalletByIdAsync Tests

    [Fact]
    public async Task GetWalletByIdAsync_WithExistingId_ReturnsWalletDto()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var wallet = CreateWallet(id: walletId, name: "Main Wallet", currencyCode: "EUR");

        SetupValidWallet(walletId, wallet);

        // Act
        var result = await _sut.GetWalletByIdAsync(walletId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(walletId);
        result.Name.Should().Be("Main Wallet");
        result.BaseCurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public async Task GetWalletByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.GetWalletByIdAsync(walletId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");
    }

    #endregion

    #region GetUserWalletsAsync Tests

    [Fact]
    public async Task GetUserWalletsAsync_ReturnsAllUserWallets()
    {
        // Arrange
        var wallets = new List<Wallet>
        {
            CreateWallet(name: "Main Wallet", currencyCode: "USD"),
            CreateWallet(name: "Savings", currencyCode: "EUR")
        };

        _walletRepositoryMock
            .Setup(r => r.GetUserWalletsAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallets);

        // Act
        var result = await _sut.GetUserWalletsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(w => w.Name == "Main Wallet" && w.BaseCurrencyCode == "USD");
        result.Should().Contain(w => w.Name == "Savings" && w.BaseCurrencyCode == "EUR");
    }

    [Fact]
    public async Task GetUserWalletsAsync_WithNoWallets_ReturnsEmptyList()
    {
        // Arrange
        _walletRepositoryMock
            .Setup(r => r.GetUserWalletsAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Wallet>());

        // Act
        var result = await _sut.GetUserWalletsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateWalletAsync Tests

    [Fact]
    public async Task CreateWalletAsync_WithValidData_ReturnsNewId()
    {
        // Arrange
        var createDto = new WalletCreateDto
        {
            Name = "Travel Fund",
            BaseCurrencyCode = "EUR"
        };

        SetupValidValidation<WalletCreateDto>();
        SetupWalletNameExists(createDto.Name, null, false);
        SetupCurrencyExists("EUR");
        SetupSuccessfulSave();

        // Act
        var result = await _sut.CreateWalletAsync(createDto);

        // Assert
        result.Should().NotBe(Guid.Empty);

        _walletRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Wallet>(w =>
                w.UserId == _defaultUserId &&
                w.Name == "Travel Fund" &&
                w.BaseCurrencyCode == "EUR"),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWalletAsync_TrimsNameWhitespace()
    {
        // Arrange
        var createDto = new WalletCreateDto
        {
            Name = "  Travel Fund  ",
            BaseCurrencyCode = "EUR"
        };

        SetupValidValidation<WalletCreateDto>();
        SetupWalletNameExists("  Travel Fund  ", null, false);
        SetupCurrencyExists("EUR");
        SetupSuccessfulSave();

        // Act
        await _sut.CreateWalletAsync(createDto);

        // Assert
        _walletRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Wallet>(w => w.Name == "Travel Fund"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWalletAsync_WithDuplicateName_ThrowsConflictException()
    {
        // Arrange
        var createDto = new WalletCreateDto
        {
            Name = "Main Wallet",
            BaseCurrencyCode = "USD"
        };

        SetupValidValidation<WalletCreateDto>();
        SetupWalletNameExists("Main Wallet", null, true);

        // Act
        var act = async () => await _sut.CreateWalletAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Main Wallet*already exists*");

        _walletRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateWalletAsync_WithUnsupportedCurrency_ThrowsConflictException()
    {
        // Arrange
        var createDto = new WalletCreateDto
        {
            Name = "Test Wallet",
            BaseCurrencyCode = "XYZ"
        };

        SetupValidValidation<WalletCreateDto>();
        SetupWalletNameExists("Test Wallet", null, false);
        SetupCurrencyExists("XYZ", false);

        // Act
        var act = async () => await _sut.CreateWalletAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*XYZ*not supported*");
    }

    #endregion

    #region UpdateWalletAsync Tests

    [Fact]
    public async Task UpdateWalletAsync_WithValidData_UpdatesWallet()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var wallet = CreateWallet(id: walletId, name: "Old Name");
        var updateDto = new WalletUpdateDto { Name = "New Name" };

        SetupValidValidation<WalletUpdateDto>();
        SetupValidWallet(walletId, wallet);
        SetupWalletNameExists("New Name", walletId, false);
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateWalletAsync(walletId, updateDto);

        // Assert
        wallet.Name.Should().Be("New Name");

        _walletRepositoryMock.Verify(r => r.Update(wallet), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWalletAsync_TrimsNameWhitespace()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var wallet = CreateWallet(id: walletId);
        var updateDto = new WalletUpdateDto { Name = "  Updated Name  " };

        SetupValidValidation<WalletUpdateDto>();
        SetupValidWallet(walletId, wallet);
        SetupWalletNameExists("Updated Name", walletId, false);
        SetupSuccessfulSave();

        // Act
        await _sut.UpdateWalletAsync(walletId, updateDto);

        // Assert
        wallet.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateWalletAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var updateDto = new WalletUpdateDto { Name = "New Name" };

        SetupValidValidation<WalletUpdateDto>();
        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.UpdateWalletAsync(walletId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");
    }

    [Fact]
    public async Task UpdateWalletAsync_WithDuplicateName_ThrowsConflictException()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var wallet = CreateWallet(id: walletId, name: "Old Name");
        var updateDto = new WalletUpdateDto { Name = "Existing Wallet" };

        SetupValidValidation<WalletUpdateDto>();
        SetupValidWallet(walletId, wallet);
        SetupWalletNameExists("Existing Wallet", walletId, true);

        // Act
        var act = async () => await _sut.UpdateWalletAsync(walletId, updateDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Existing Wallet*already exists*");

        _walletRepositoryMock.Verify(r => r.Update(It.IsAny<Wallet>()), Times.Never);
    }

    #endregion

    #region DeleteWalletAsync Tests

    [Fact]
    public async Task DeleteWalletAsync_WithExistingId_DeletesWallet()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var wallet = CreateWallet(id: walletId);

        SetupValidWallet(walletId, wallet);
        SetupSuccessfulSave();

        // Act
        await _sut.DeleteWalletAsync(walletId);

        // Assert
        _walletRepositoryMock.Verify(r => r.SoftDelete(wallet), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteWalletAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        _walletRepositoryMock
            .Setup(r => r.GetByIdForUserAsync(_defaultUserId, walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var act = async () => await _sut.DeleteWalletAsync(walletId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{walletId}*");

        _walletRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Wallet>()), Times.Never);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task CreateWalletAsync_WhenUserContextThrows_PropagatesException()
    {
        // Arrange
        var createDto = new WalletCreateDto
        {
            Name = "Test",
            BaseCurrencyCode = "USD"
        };

        _userContextMock.Setup(c => c.GetRequiredUserId())
            .Throws(new UnauthorizedAccessException("User not authenticated"));

        // Act
        var act = async () => await _sut.CreateWalletAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion
}