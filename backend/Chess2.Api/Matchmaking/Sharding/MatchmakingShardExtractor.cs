using Akka.Cluster.Sharding;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Matchmaking.Sharding;

public class MatchmakingShardExtractor(AppSettings settings)
    : HashCodeMessageExtractor(settings.Akka.MatchmakingShardCount)
{
    public override string? EntityId(object message)
    {
        if (message is not IMatchmakingMessage matchmakingMessage)
            return null;

        var entityId =
            $"matchmaking:{matchmakingMessage.TimeControl.BaseMinutes}+{matchmakingMessage.TimeControl.Increment}";
        return entityId;
    }
}
