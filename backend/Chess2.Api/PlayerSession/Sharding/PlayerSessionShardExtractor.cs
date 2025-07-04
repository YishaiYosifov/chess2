using Akka.Cluster.Sharding;
using Chess2.Api.PlayerSession.Models;

namespace Chess2.Api.PlayerSession.Sharding;

public class PlayerSessionShardExtractor(int shardCount) : HashCodeMessageExtractor(shardCount)
{
    public override string? EntityId(object message)
    {
        return message is IPlayerSessionCommand playerSessionCommand
            ? playerSessionCommand.UserId
            : null;
    }
}
