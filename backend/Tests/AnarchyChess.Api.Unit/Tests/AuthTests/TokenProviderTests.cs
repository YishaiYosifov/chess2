using System.Security.Claims;
using System.Text;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.AuthTests;

public class TokenProviderTests
{
    private readonly TokenProvider _tokenProvider;
    private readonly AuthSettings _settings;

    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    public TokenProviderTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _tokenProvider = new(Options.Create(settings), _timeProviderMock);
        _settings = settings.Auth;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
    }

    [Fact]
    public void GenerateAccessToken_sets_expected_claims()
    {
        var user = new AuthedUserFaker().Generate();

        var result = _tokenProvider.GenerateAccessToken(user);

        result.IsError.Should().BeFalse();

        var token = ParseJwt(result.Value);
        token
            .Claims.Should()
            .Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == user.Id);
        token.Claims.Should().Contain(x => x.Type == "type" && x.Value == "access");

        token
            .ValidTo.Should()
            .BeCloseTo(_fakeNow.Add(_settings.AccessMaxAge).UtcDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GenerateAccessToken_tokens_have_valid_signature()
    {
        var user = new AuthedUserFaker().Generate();
        var handler = new JsonWebTokenHandler();

        var tokenString = _tokenProvider.GenerateAccessToken(user).Value;

        var validation = await handler.ValidateTokenAsync(
            tokenString,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.Jwt.SecretKey)
                ),
            }
        );

        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAccessToken_sets_correct_issuer_and_audience()
    {
        var user = new AuthedUserFaker().Generate();

        var result = _tokenProvider.GenerateAccessToken(user);

        result.IsError.Should().BeFalse();

        var token = ParseJwt(result.Value);
        token.Issuer.Should().Be(_settings.Jwt.Issuer);
        token.Audiences.Should().BeEquivalentTo([_settings.Jwt.Audience]);
    }

    [Fact]
    public void GenerateAccessToken_rejects_banned_users()
    {
        var user = new AuthedUserFaker().RuleFor(x => x.IsBanned, true).Generate();

        var result = _tokenProvider.GenerateAccessToken(user);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.UserBanned);
    }

    [Fact]
    public void GenerateRefreshToken_sets_expected_claims()
    {
        var user = new AuthedUserFaker().Generate();
        var jti = Guid.NewGuid().ToString();

        var token = ParseJwt(_tokenProvider.GenerateRefreshToken(user, jti));

        token
            .Claims.Should()
            .Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == user.Id);
        token.Claims.Should().Contain(x => x.Type == "type" && x.Value == "refresh");
        token.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Jti && x.Value == jti);

        token
            .ValidTo.Should()
            .BeCloseTo(_fakeNow.Add(_settings.RefreshMaxAge).UtcDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateGuestToken_sets_expected_claims()
    {
        var guestId = UserId.Guest();

        var token = ParseJwt(_tokenProvider.GenerateGuestToken(guestId));

        token
            .Claims.Should()
            .Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == guestId);
        token.Claims.Should().Contain(x => x.Type == "type" && x.Value == "access");
        token.Claims.Should().Contain(x => x.Type == ClaimTypes.Anonymous && x.Value == "true");

        token.ValidTo.Should().Be(DateTime.MaxValue);
    }

    private static JsonWebToken ParseJwt(string token) =>
        new JsonWebTokenHandler().ReadJsonWebToken(token);
}
