using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chess2.Api.Auth.Errors;
using Chess2.Api.Auth.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using ErrorOr;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests;

public class AuthServiceTests : BaseIntegrationTest
{
    private readonly IAuthService _authService;

    public AuthServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _authService = Scope.ServiceProvider.GetRequiredService<IAuthService>();
    }

    [Fact]
    public async Task SignupAsync_adds_a_user_to_the_database()
    {
        const string username = "testuser";
        const string email = "test@example.com";
        const string countryCode = "US";
        var result = await _authService.SignupAsync(username, email, countryCode);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        var resultUser = result.Value;
        resultUser.UserName.Should().Be(username);
        resultUser.Email.Should().Be(email);
        resultUser.CountryCode.Should().Be(countryCode);

        var fromDb = await DbContext.Users.FirstOrDefaultAsync(x => x.UserName == username);
        fromDb.Should().NotBeNull();
        fromDb.Should().BeEquivalentTo(resultUser);
    }

    [Fact]
    public async Task GetLoggedInUserAsync_returns_the_user_from_the_claims()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var principal = ClaimUtils.CreateUserClaims(user.Id);

        var result = await _authService.GetLoggedInUserAsync(principal);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetLoggedInUserAsync_returns_an_error_when_the_claims_are_null()
    {
        var result = await _authService.GetLoggedInUserAsync(null);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(Error.Unauthorized());
    }

    [Fact]
    public async Task GetLoggedInUserAsync_returns_an_error_when_the_user_is_not_found()
    {
        await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var principal = ClaimUtils.CreateUserClaims("69420");

        var result = await _authService.GetLoggedInUserAsync(principal);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(Error.Unauthorized());
    }

    [Fact]
    public async Task GenerateAuthTokensAsync_creates_valid_tokens()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var result = await _authService.GenerateAuthTokensAsync(user);
        await DbContext.SaveChangesAsync();

        result.Should().NotBeNull();
        AuthUtils.AuthenticateWithTokens(ApiClient, result.AccessToken, result.RefreshToken);
        await AuthTestUtils.AssertAuthenticated(ApiClient);
        var refreshResult = await ApiClient.Api.RefreshTokenAsync();
        refreshResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_handles_a_valid_refresh_tokens_correctly()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var refreshToken = await FakerUtils.StoreFakerAsync(DbContext, new RefreshTokenFaker(user));

        var principal = ClaimUtils.CreateUserClaims(
            user.Id,
            new Claim(JwtRegisteredClaimNames.Jti, refreshToken.Jti)
        );

        var result = await _authService.RefreshTokenAsync(principal);

        var updatedToken = await DbContext.RefreshTokens.FindAsync(refreshToken.Id);
        updatedToken.Should().NotBeNull();
        updatedToken.IsRevoked.Should().BeTrue();

        result.IsError.Should().BeFalse();
        var tokens = result.Value;

        AuthUtils.AuthenticateWithTokens(ApiClient, tokens.AccessToken, tokens.RefreshToken);
        await AuthTestUtils.AssertAuthenticated(ApiClient);
        var refreshResult = await ApiClient.Api.RefreshTokenAsync();
        refreshResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_returns_an_error_when_jti_claim_is_not_found()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(DbContext, new RefreshTokenFaker(user));

        var principal = ClaimUtils.CreateUserClaims(
            user.Id,
            new Claim(JwtRegisteredClaimNames.Jti, "some-random-jti")
        );

        var result = await _authService.RefreshTokenAsync(principal);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(AuthErrors.TokenInvalid);
    }

    [Fact]
    public async Task RefreshTokenAsync_returns_an_error_when_claims_principal_is_null()
    {
        var result = await _authService.RefreshTokenAsync(null);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(Error.Unauthorized());
    }
}
