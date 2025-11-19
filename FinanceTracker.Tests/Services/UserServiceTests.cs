using FinanceTracker.Application.DTOs.User;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceTracker.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<UserUpdateDto>> _updateValidatorMock;
    private readonly Mock<IValidator<ChangePasswordRequest>> _passwordValidatorMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _sut;

    private readonly Guid _defaultUserId = Guid.NewGuid();

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userContextMock = new Mock<IUserContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _updateValidatorMock = new Mock<IValidator<UserUpdateDto>>();
        _passwordValidatorMock = new Mock<IValidator<ChangePasswordRequest>>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        SetupUserContext(_defaultUserId);

        _sut = new UserService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _updateValidatorMock.Object,
            _passwordValidatorMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private void SetupUserContext(Guid userId)
    {
        _userContextMock.Setup(c => c.GetRequiredUserId()).Returns(userId);
    }

    private User CreateUser(
        Guid? id = null,
        string userName = "testuser",
        string passwordHash = "hashedpassword123")
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            UserName = userName,
            PasswordHash = passwordHash,
            IsDeleted = false
        };
    }

    private void SetupValidValidation<T>()
    {
        if (typeof(T) == typeof(UserUpdateDto))
        {
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<UserUpdateDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
        else if (typeof(T) == typeof(ChangePasswordRequest))
        {
            _passwordValidatorMock
                .Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<ChangePasswordRequest>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }
    }

    private void SetupSuccessfulSave()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, userName: "johndoe");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.UserName.Should().Be("johndoe");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.GetUserByIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateUser(userName: "user1"),
            CreateUser(userName: "user2"),
            CreateUser(userName: "user3")
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.UserName == "user1");
        result.Should().Contain(u => u.UserName == "user2");
        result.Should().Contain(u => u.UserName == "user3");
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, userName: "oldusername");
        var updateDto = new UserUpdateDto("newusername");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupValidValidation<UserUpdateDto>();

        _userRepositoryMock
            .Setup(r => r.IsUserNameTakenAsync(userId, "newusername", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        SetupSuccessfulSave();

        // Act
        await _sut.UpdateUserAsync(userId, updateDto);

        // Assert
        user.UserName.Should().Be("newusername");

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UserUpdateDto("newusername");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.UpdateUserAsync(userId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");
    }

    [Fact]
    public async Task UpdateUserAsync_WithTakenUserName_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, userName: "oldusername");
        var updateDto = new UserUpdateDto("takenusername");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupValidValidation<UserUpdateDto>();

        _userRepositoryMock
            .Setup(r => r.IsUserNameTakenAsync(userId, "takenusername", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.UpdateUserAsync(userId, updateDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*takenusername*already taken*");

        _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithExistingId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupSuccessfulSave();

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert
        _userRepositoryMock.Verify(r => r.SoftDelete(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.DeleteUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");

        _userRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidPassword_ChangesPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, passwordHash: "oldhash");
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword123"
        };

        SetupUserContext(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupValidValidation<ChangePasswordRequest>();

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("oldpassword", "oldhash"))
            .Returns(true);

        _passwordHasherMock
            .Setup(h => h.HashPassword("newpassword123"))
            .Returns("newhash");

        SetupSuccessfulSave();

        // Act
        await _sut.ChangePasswordAsync(request);

        // Assert
        user.PasswordHash.Should().Be("newhash");

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, passwordHash: "oldhash");
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword123"
        };

        SetupUserContext(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupValidValidation<ChangePasswordRequest>();

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("wrongpassword", "oldhash"))
            .Returns(false);

        // Act
        var act = async () => await _sut.ChangePasswordAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Current password is incorrect*");

        _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword123"
        };

        SetupUserContext(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.ChangePasswordAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task ChangePasswordAsync_WhenUserContextThrows_PropagatesException()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword123"
        };

        _userContextMock.Setup(c => c.GetRequiredUserId())
            .Throws(new UnauthorizedAccessException("User not authenticated"));

        // Act
        var act = async () => await _sut.ChangePasswordAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion
}