using Akka.Cluster.Sharding;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Matchmaking.Sharding;

public class MatchmakingShardExtractor(AppSettings settings)
    : HashCodeMessageExtractor(settings.Akka.MatchmakingShardCount)
{
    public override string? EntityId(object message)
    {
        return message.ToString();
    }
}
