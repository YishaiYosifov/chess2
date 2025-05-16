using AutoFixture;
using Chess2.Api.Errors;
using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using Chess2.Api.Services.Auth;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests;

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
        var result = await _refreshTokenService.CreateRefreshTokenAsync(_user);

        await _refreshTokenRepositoryMock.Received(1).AddRefreshTokenAsync(result);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_sets_the_correct_expirery_time()
    {
        var now = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(now);

        var result = await _refreshTokenService.CreateRefreshTokenAsync(_user);

        result.ExpiresAt.Should().Be(now.AddDays(_appSettings.Jwt.RefreshExpiresInDays));
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_generates_a_different_jti_every_time()
    {
        var firstResult = await _refreshTokenService.CreateRefreshTokenAsync(_user);

        firstResult.Jti.Should().HaveLength(36);

        var secondResult = await _refreshTokenService.CreateRefreshTokenAsync(_user);

        secondResult.Jti.Should().HaveLength(36);
        firstResult.Jti.Should().NotBe(secondResult.Jti);
    }

    [Fact]
    public async Task ConsumeRefreshTokenAsync_returns_an_error_when_refresh_token_doesnt_exist()
    {
        var result = await _refreshTokenService.ConsumeRefreshTokenAsync(_user, "some jti");

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(AuthErrors.TokenInvalid);
    }
}
