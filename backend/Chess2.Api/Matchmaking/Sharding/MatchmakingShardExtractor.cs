using Akka.Cluster.Sharding;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Sharding;

public class MatchmakingShardExtractor(int shardCount) : HashCodeMessageExtractor(shardCount)
{
    public override string? EntityId(object message)
    {
        if (message is not IMatchmakingCommand matchmakingMessage)
            return null;

        return matchmakingMessage.TimeControl.ToShortString();
    }
}
