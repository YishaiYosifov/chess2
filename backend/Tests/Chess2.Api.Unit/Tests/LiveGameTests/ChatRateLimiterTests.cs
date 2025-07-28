using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class ChatRateLimiterTests
{
    private readonly ChatSettings _settings;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private DateTimeOffset _dateMock = DateTimeOffset.UtcNow;

    private readonly ChatRateLimiter _limiter;

    public ChatRateLimiterTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Game.Chat;
        _timeProviderMock.GetUtcNow().Returns(_dateMock);

        _limiter = new ChatRateLimiter(Options.Create(settings), _timeProviderMock);
    }

    [Fact]
    public void ShouldDenyRequest_WhenBucketIsFull()
    {
        for (int i = 0; i < _settings.BucketCapacity; i++)
        {
            _limiter.ShouldAllowRequest("user1").Should().BeTrue();
        }

        _limiter.ShouldAllowRequest("user1").Should().BeFalse();
    }

    [Fact]
    public void ShouldAllowRequest_AfterRefill()
    {
        for (int i = 0; i < _settings.BucketCapacity; i++)
            _limiter.ShouldAllowRequest("user1").Should().BeTrue();
        _limiter.ShouldAllowRequest("user1").Should().BeFalse();

        AdvanceTime(_settings.BucketRefillRate);

        _limiter.ShouldAllowRequest("user1").Should().BeTrue();
    }

    [Fact]
    public void ShouldResetBucketForNewUser()
    {
        for (int i = 0; i < _settings.BucketCapacity; i++)
            _limiter.ShouldAllowRequest("user1").Should().BeTrue();
        _limiter.ShouldAllowRequest("user1").Should().BeFalse();

        _limiter.ShouldAllowRequest("user2").Should().BeTrue();
    }

    private void AdvanceTime(TimeSpan duration)
    {
        _dateMock = _dateMock.Add(duration);
        _timeProviderMock.GetUtcNow().Returns(_dateMock);
    }
}
