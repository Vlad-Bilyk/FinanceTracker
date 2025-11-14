using FinanceTracker.Application.DTOs.User;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UserUpdateDto> _updateValidator;
    private readonly IValidator<ChangePasswordRequest> _passwordValidator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserContext _userContext;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher,
        IValidator<UserUpdateDto> updateValidator, IValidator<ChangePasswordRequest> passwordValidator,
        IUserContext userContext, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"User with id {id} was not found");

        return new UserDto(user.Id, user.UserName);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(ct);

        return users.Select(user => new UserDto(user.Id, user.UserName)).ToList();
    }

    public async Task UpdateUserAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"User with id {id} was not found");

        await _updateValidator.ValidateAndThrowAsync(updateDto, ct);

        if (await _unitOfWork.Users.IsUserNameTakenAsync(id, updateDto.UserName, ct))
        {
            throw new ConflictException($"UserName '{updateDto.UserName}' is already taken");
        }

        user.UserName = updateDto.UserName;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User with id {UserId} updated", id);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"User with id {id} was not found");

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User with id {UserId} deleted", id);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        var userId = _userContext.UserId
            ?? throw new UnauthorizedException("User is not authenticated");

        var user = await _unitOfWork.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found");

        await _passwordValidator.ValidateAndThrowAsync(request, ct);

        var verifyResult = _passwordHasher
            .VerifyPassword(request.CurrentPassword, user.PasswordHash);

        if (!verifyResult)
        {
            throw new ValidationException("Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed for user with id {UserId}", user.Id);
    }
}
