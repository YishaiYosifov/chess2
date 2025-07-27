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
    public void RegisterMessage_up_to_limit_does_not_trigger_cooldown()
    {
        var userId = "user1";

        RegisterMessages(userId, _settings.MaxMessagesPerWindow);

        _limiter.IsOnCooldown(userId).Should().BeFalse();
    }

    [Fact]
    public void RegisterMessage_exceeding_limit_triggers_cooldown()
    {
        var userId = "user1";

        RegisterMessages(userId, _settings.MaxMessagesPerWindow + 1);

        _limiter.IsOnCooldown(userId).Should().BeTrue();
    }

    [Fact]
    public void IsOnCooldown_returns_false_after_cooldown_is_over()
    {
        var userId = "user1";

        RegisterMessages(userId, _settings.MaxMessagesPerWindow + 1);
        AdvanceTime(_settings.OffenseCooldownDuration + TimeSpan.FromSeconds(1));

        _limiter.IsOnCooldown(userId).Should().BeFalse();
    }

    [Fact]
    public void RegisterMessage_cleans_up_old_messages_outside_of_window()
    {
        var userId = "user1";

        RegisterMessages(userId, _settings.MaxMessagesPerWindow);
        AdvanceTime(_settings.RateLimitWindow + TimeSpan.FromSeconds(1));
        RegisterMessages(userId, _settings.MaxMessagesPerWindow);

        _limiter.IsOnCooldown(userId).Should().BeFalse();
    }

    [Fact]
    public void RegisterMessage_and_IsOnCooldown_with_multiple_users()
    {
        var user1 = "user1";
        var user2 = "user2";

        RegisterMessages(user1, _settings.MaxMessagesPerWindow + 1);
        RegisterMessages(user2, _settings.MaxMessagesPerWindow - 1);

        _limiter.IsOnCooldown(user1).Should().BeTrue();
        _limiter.IsOnCooldown(user2).Should().BeFalse();

        AdvanceTime(_settings.OffenseCooldownDuration + TimeSpan.FromSeconds(1));

        _limiter.IsOnCooldown(user1).Should().BeFalse();
        _limiter.IsOnCooldown(user2).Should().BeFalse();
    }

    private void RegisterMessages(string userId, int count)
    {
        for (var i = 0; i < count; i++)
            _limiter.RegisterMessage(userId);
    }

    private void AdvanceTime(TimeSpan duration)
    {
        _dateMock = _dateMock.Add(duration);
        _timeProviderMock.GetUtcNow().Returns(_dateMock);
    }
}
