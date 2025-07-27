using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Services;

public interface IChatRateLimiter
{
    bool IsOnCooldown(string userId);
    void RegisterMessage(string userId);
}

public class ChatRateLimiter(IOptions<AppSettings> settings, TimeProvider timeProvider)
    : IChatRateLimiter
{
    private readonly ChatSettings _settings = settings.Value.Game.Chat;
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly Dictionary<string, List<DateTimeOffset>> _userMessageTimestamps = [];
    private readonly Dictionary<string, DateTimeOffset> _cooldownExpirations = [];

    public void RegisterMessage(string userId)
    {
        var latestMessages = _userMessageTimestamps.GetValueOrDefault(userId, []);
        var now = _timeProvider.GetUtcNow();

        var effectiveMessages = latestMessages
            .Where(sentAt => now - sentAt <= _settings.RateLimitWindow)
            .ToList();
        effectiveMessages.Add(now);
        _userMessageTimestamps[userId] = effectiveMessages;

        if (effectiveMessages.Count > _settings.MaxMessagesPerWindow)
            AddToCooldown(userId);
    }

    private void AddToCooldown(string userId)
    {
        _cooldownExpirations[userId] =
            _timeProvider.GetUtcNow() + _settings.OffenseCooldownDuration;
    }

    public bool IsOnCooldown(string userId)
    {
        if (!_cooldownExpirations.TryGetValue(userId, out var cooldownEnd))
            return false;

        if (_timeProvider.GetUtcNow() < cooldownEnd)
            return true;

        _cooldownExpirations.Remove(userId);
        return false;
    }
}
