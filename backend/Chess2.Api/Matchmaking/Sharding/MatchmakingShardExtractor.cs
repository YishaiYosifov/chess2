using Akka.Cluster.Sharding;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Matchmaking.Sharding;

public class MatchmakingShardExtractor : HashCodeMessageExtractor
{
    public MatchmakingShardExtractor(AppSettings settings)
        : base(settings.Akka.MatchmakingShardCount) { }

    public override string? EntityId(object message)
    {
        throw new NotImplementedException();
    }
}
