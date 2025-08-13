using Chess2.Api.Auth.Errors;
using Chess2.Api.Auth.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Models;
using ErrorOr;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Chess2.Api.Integration.Tests.AuthTests;

public class AuthServiceTests : BaseIntegrationTest
{
    private readonly IAuthService _authService;

    public AuthServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _authService = Scope.ServiceProvider.GetRequiredService<IAuthService>();
    }

    [Fact]
    public void GetUserId_returns_id_from_claim()
    {
        const string id = "test-id";
        var claims = ClaimUtils.CreateClaimsPrincipal([new Claim(ClaimTypes.NameIdentifier, id)]);

        var result = _authService.GetUserId(claims);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(id);
    }

    [Fact]
    public void GetUserId_returns_an_error_when_claim_is_missing()
    {
        var claims = ClaimUtils.CreateClaimsPrincipal(
            [new Claim(ClaimTypes.Name, "something random")]
        );

        var result = _authService.GetUserId(claims);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Error.Unauthorized());
    }

    [Fact]
    public async Task MatchAuthTypeAsync_gets_the_user_id_when_guest()
    {
        UserId userId = "test guest id";
        var claims = ClaimUtils.CreateGuestClaims(userId);

        var result = await _authService.MatchAuthTypeAsync(
            claims,
            whenAuthed: x => throw new ArgumentException("wrong user type matched"),
            whenGuest: x => Task.FromResult(x)
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(userId);
    }

    [Fact]
    public async Task MatchAuthTypeAsync_gets_the_user_when_authed()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);
        var claims = ClaimUtils.CreateUserClaims(user.Id);

        var result = await _authService.MatchAuthTypeAsync(
            claims,
            whenAuthed: x => Task.FromResult(x),
            whenGuest: x => throw new ArgumentException("wrong user type matched")
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task MatchAuthTypeAsync_returns_error_when_userid_invalid()
    {
        ClaimsPrincipal? claims = null;
        var result = await _authService.MatchAuthTypeAsync(
            claims,
            whenAuthed: Task.FromResult<object>,
            whenGuest: x => Task.FromResult<object>(x)
        );

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(Error.Unauthorized());
    }

    [Fact]
    public async Task MatchAuthTypeAsync_returns_error_when_authed_user_not_found()
    {
        var claims = ClaimUtils.CreateUserClaims("nonexistent-user-id");

        var result = await _authService.MatchAuthTypeAsync(
            claims,
            whenAuthed: x => Task.FromResult(x),
            whenGuest: x => throw new ArgumentException("wrong user type matched")
        );

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(Error.Unauthorized());
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

        var fromDb = await DbContext.Users.FirstOrDefaultAsync(x => x.UserName == username, CT);
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

        var result = await _authService.GenerateAuthTokensAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

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

        var result = await _authService.RefreshTokenAsync(principal, CT);

        var updatedToken = await DbContext.RefreshTokens.FindAsync([refreshToken.Id], CT);
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

        var result = await _authService.RefreshTokenAsync(principal, CT);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(AuthErrors.TokenInvalid);
    }

    [Fact]
    public async Task RefreshTokenAsync_returns_an_error_when_claims_principal_is_null()
    {
        var result = await _authService.RefreshTokenAsync(null, CT);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(Error.Unauthorized());
    }
}
