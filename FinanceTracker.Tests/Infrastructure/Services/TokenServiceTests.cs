using FinanceTracker.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace FinanceTracker.Tests.Infrastructure.Services;

public class TokenServiceTests
{
    private const string JwtSectionName = "JwtSettings";
    private const string SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!!";
    private const string Issuer = "FinanceTrackerIssuer";
    private const string Audience = "FinanceTrackerAudience";
    private const double ExpirationHours = 1;

    #region Helper Methods

    private static IConfiguration CreateConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            [$"{JwtSectionName}:SecretKey"] = SecretKey,
            [$"{JwtSectionName}:Issuer"] = Issuer,
            [$"{JwtSectionName}:Audience"] = Audience,
            [$"{JwtSectionName}:ExpirationHours"] =
                ExpirationHours.ToString(CultureInfo.InvariantCulture)
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    #endregion

    [Fact]
    public void GenerateToken_WhenInputIsValid_ReturnsValidJwtToken()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var tokenService = new TokenService(configuration);
        var userId = Guid.NewGuid();
        const string username = "testUser";

        // Act
        var token = tokenService.GenerateToken(userId, username);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        jwtToken.Issuer.Should().Be(Issuer);
        jwtToken.Audiences.Should().Contain(Audience);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);

        userIdClaim.Should().NotBeNull();
        nameClaim.Should().NotBeNull();

        userIdClaim!.Value.Should().Be(userId.ToString());
        nameClaim!.Value.Should().Be(username);

        var expectedExpiration = DateTime.UtcNow.AddHours(ExpirationHours);
        var difference = (jwtToken.ValidTo - expectedExpiration).Duration();
        difference.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_WhenSecretKeyIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            [$"{JwtSectionName}:Issuer"] = Issuer,
            [$"{JwtSectionName}:Audience"] = Audience,
            [$"{JwtSectionName}:ExpirationHours"] =
                ExpirationHours.ToString(CultureInfo.InvariantCulture)
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var tokenService = new TokenService(configuration);

        // Act
        var act = () => tokenService.GenerateToken(Guid.NewGuid(), "testUser");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT Secret Key not configured");
    }

    [Fact]
    public void GenerateToken_WhenExpirationHoursSettingIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            [$"{JwtSectionName}:SecretKey"] = SecretKey,
            [$"{JwtSectionName}:Issuer"] = Issuer,
            [$"{JwtSectionName}:Audience"] = Audience
            // No ExpirationHours key
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var tokenService = new TokenService(configuration);

        // Act
        var act = () => tokenService.GenerateToken(Guid.NewGuid(), "testUser");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT expiration Hours not configured");
    }

    [Fact]
    public void GenerateToken_WhenExpirationHoursSettingIsInvalid_ThrowsFormatException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            [$"{JwtSectionName}:SecretKey"] = SecretKey,
            [$"{JwtSectionName}:Issuer"] = Issuer,
            [$"{JwtSectionName}:Audience"] = Audience,
            [$"{JwtSectionName}:ExpirationHours"] = "NotANumber"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var tokenService = new TokenService(configuration);

        // Act
        var act = () => tokenService.GenerateToken(Guid.NewGuid(), "testUser");

        // Assert
        act.Should().Throw<FormatException>();
    }
}
