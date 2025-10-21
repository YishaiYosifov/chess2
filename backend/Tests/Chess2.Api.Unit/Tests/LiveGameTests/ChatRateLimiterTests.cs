using Chess2.Api.Game.Services;
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
    public void ShouldAllowRequest_denies_request_when_the_bucket_is_full()
    {
        TimeSpan cooldown;

        for (int i = 0; i < _settings.BucketCapacity; i++)
        {
            var allowed = _limiter.ShouldAllowRequest("user1", out cooldown);
            allowed.Should().BeTrue();
            cooldown.Should().Be(TimeSpan.Zero);
        }

        // cooldown started, allow last message
        _limiter.ShouldAllowRequest("user1", out cooldown).Should().BeTrue();
        cooldown.Should().Be(_settings.BucketRefillRate);

        // cooldown already notified, so disallow message
        _limiter.ShouldAllowRequest("user1", out cooldown).Should().BeFalse();
        cooldown.Should().Be(_settings.BucketRefillRate);
    }

    [Fact]
    public void ShouldAllowRequest_allows_requests_once_the_bucket_refills()
    {
        for (int i = 0; i < _settings.BucketCapacity + 1; i++)
        {
            _limiter.ShouldAllowRequest("user1", out _).Should().BeTrue();
        }
        _limiter.ShouldAllowRequest("user1", out var cooldown).Should().BeFalse();
        cooldown.Should().Be(_settings.BucketRefillRate);

        var halfRefillRate = _settings.BucketRefillRate / 2;
        AdvanceTime(halfRefillRate);
        _limiter.ShouldAllowRequest("user1", out cooldown).Should().BeFalse();
        cooldown.Should().Be(halfRefillRate);
        AdvanceTime(halfRefillRate);

        _limiter.ShouldAllowRequest("user1", out cooldown).Should().BeTrue();
        cooldown.Should().Be(_settings.BucketRefillRate);

        AdvanceTime(_settings.BucketRefillRate * 2);
        _limiter.ShouldAllowRequest("user1", out cooldown).Should().BeTrue();
        cooldown.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ShouldAllowRequest_can_handle_multiple_users()
    {
        for (int i = 0; i < _settings.BucketCapacity + 1; i++)
            _limiter.ShouldAllowRequest("user1", out _).Should().BeTrue();
        _limiter.ShouldAllowRequest("user1", out _).Should().BeFalse();

        _limiter.ShouldAllowRequest("user2", out var cooldown).Should().BeTrue();
        cooldown.Should().Be(TimeSpan.Zero);
    }

    private void AdvanceTime(TimeSpan duration)
    {
        _dateMock = _dateMock.Add(duration);
        _timeProviderMock.GetUtcNow().Returns(_dateMock);
    }
}
