using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
        _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, ct);

        if (await _unitOfWork.Users.IsUserNameTakenAsync(null, request.UserName, ct))
        {
            throw new ConflictException($"UserName '{request.UserName}' is already taken");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            PasswordHash = passwordHash
        };

        await _unitOfWork.Users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User registered successfully: {UserName}", request.UserName);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await _unitOfWork.Users.GetByUserNameAsync(request.UserName, ct);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed invalid credentials");
            throw new UnauthorizedException("Invalid username or password");
        }

        _logger.LogInformation("Login succeded: {UserId}", user.Id);

        var token = _tokenService.GenerateToken(user.Id, user.UserName);
        return new AuthResponse(token);
    }
}
