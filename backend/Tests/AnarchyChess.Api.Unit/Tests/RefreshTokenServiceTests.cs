using AutoFixture;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Repositories;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.Profile.Entities;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests;

public class RefreshTokenServiceTests : BaseUnitTest
{
    private readonly ILogger<RefreshTokenService> _loggerMock = Substitute.For<
        ILogger<RefreshTokenService>
    >();
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock =
        Substitute.For<IRefreshTokenRepository>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly AppSettings _appSettings;

    private readonly RefreshTokenService _refreshTokenService;

    private readonly AuthedUser _user;

    public RefreshTokenServiceTests()
    {
        var appSettingsOptions = Fixture.Create<IOptions<AppSettings>>();
        _appSettings = appSettingsOptions.Value;

        _refreshTokenService = new(
            _loggerMock,
            appSettingsOptions,
            _refreshTokenRepositoryMock,
            _timeProviderMock
        );

        _user = new AuthedUserFaker().Generate();
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_calls_refresh_token_repository()
    {
        var result = await _refreshTokenService.CreateRefreshTokenAsync(_user, CT);

        await _refreshTokenRepositoryMock.Received(1).AddRefreshTokenAsync(result, CT);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_sets_the_correct_expirery_time()
    {
        var now = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(now);

        var result = await _refreshTokenService.CreateRefreshTokenAsync(_user, CT);

        result.ExpiresAt.Should().Be(now.Add(_appSettings.Jwt.RefreshMaxAge));
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_generates_a_different_jti_every_time()
    {
        var firstResult = await _refreshTokenService.CreateRefreshTokenAsync(_user, CT);

        firstResult.Jti.Should().HaveLength(36);

        var secondResult = await _refreshTokenService.CreateRefreshTokenAsync(_user, CT);

        secondResult.Jti.Should().HaveLength(36);
        firstResult.Jti.Should().NotBe(secondResult.Jti);
    }

    [Fact]
    public async Task ConsumeRefreshTokenAsync_returns_an_error_when_the_refresh_token_doesnt_exist()
    {
        var result = await _refreshTokenService.ConsumeRefreshTokenAsync(_user, "some jti", CT);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(AuthErrors.TokenInvalid);
    }

    [Fact]
    public async Task ConsumeRefreshTokenAsync_returns_an_error_when_the_refresh_token_is_revoked()
    {
        var refreshToken = new RefreshTokenFaker(_user).RuleFor(x => x.IsRevoked, true).Generate();
        _refreshTokenRepositoryMock
            .GetTokenByJtiAsync(refreshToken.Jti, Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        var result = await _refreshTokenService.ConsumeRefreshTokenAsync(
            _user,
            refreshToken.Jti,
            CT
        );

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(AuthErrors.TokenInvalid);
    }

    [Fact]
    public async Task ConsumeRefreshTokenAsync_revokes_the_refresh_token()
    {
        var refreshToken = new RefreshTokenFaker(_user).Generate();
        _refreshTokenRepositoryMock
            .GetTokenByJtiAsync(refreshToken.Jti, Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        var result = await _refreshTokenService.ConsumeRefreshTokenAsync(
            _user,
            refreshToken.Jti,
            CT
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        refreshToken.IsRevoked.Should().BeTrue();
    }
}
