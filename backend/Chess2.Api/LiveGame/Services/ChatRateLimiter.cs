using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Services;

public interface IChatRateLimiter
{
    bool ShouldAllowRequest(string userId);
}

public class ChatRateLimiter(IOptions<AppSettings> settings, TimeProvider timeProvider)
    : IChatRateLimiter
{
    private readonly ChatSettings _settings = settings.Value.Game.Chat;
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly Dictionary<string, (double Fill, DateTimeOffset LastRefill)> _userBuckets = [];

    public bool ShouldAllowRequest(string userId)
    {
        var now = _timeProvider.GetUtcNow();
        if (!_userBuckets.TryGetValue(userId, out var bucket))
            bucket = (Fill: 0, LastRefill: now);

        var ellapsed = now - bucket.LastRefill;
        var tokensToAdd = (int)(ellapsed / _settings.BucketRefillRate);
        var newFill = Math.Max(0, bucket.Fill - tokensToAdd);

        if (newFill >= _settings.BucketCapacity)
        {
            _userBuckets[userId] = (newFill, bucket.LastRefill);
            return false;
        }

        _userBuckets[userId] = (newFill + 1, now);
        return true;
    }
}
