using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Services;

public interface IChatRateLimiter
{
    bool ShouldAllowRequest(UserId userId, out TimeSpan cooldownLeft);
}

public class ChatRateLimiter(IOptions<AppSettings> settings, TimeProvider timeProvider)
    : IChatRateLimiter
{
    private readonly ChatSettings _settings = settings.Value.Game.Chat;
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly Dictionary<UserId, (int Fill, DateTimeOffset LastRefill)> _userBuckets = [];

    public bool ShouldAllowRequest(UserId userId, out TimeSpan cooldownLeft)
    {
        var now = _timeProvider.GetUtcNow();
        if (!_userBuckets.TryGetValue(userId, out var bucket))
            bucket = (Fill: 0, LastRefill: now);

        var ellapsed = now - bucket.LastRefill;
        var tokensToAdd = (int)(ellapsed / _settings.BucketRefillRate);
        var availableTokens = Math.Max(0, bucket.Fill - tokensToAdd);

        bool isWithinCapacity = availableTokens < _settings.BucketCapacity;
        bool isAtCapacity = availableTokens == _settings.BucketCapacity;

        if (isWithinCapacity)
        {
            _userBuckets[userId] = (availableTokens + 1, now);
            cooldownLeft = TimeSpan.Zero;
            return true;
        }
        else if (isAtCapacity)
        {
            // this message fills the bucket completely, cooldown starts now
            _userBuckets[userId] = (availableTokens + 1, now);
            cooldownLeft = _settings.BucketRefillRate;
            return true;
        }
        else
        {
            // cooldown is not fully active, disallow the request
            _userBuckets[userId] = (availableTokens, bucket.LastRefill);
            var nextTokenAvailableAt = bucket.LastRefill + _settings.BucketRefillRate;
            cooldownLeft = nextTokenAvailableAt - now;
            return false;
        }
    }
}
