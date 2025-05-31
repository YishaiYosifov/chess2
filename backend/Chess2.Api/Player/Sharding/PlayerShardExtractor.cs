using Akka.Cluster.Sharding;
using Chess2.Api.Player.Models;

namespace Chess2.Api.Player.Sharding;

public class PlayerShardExtractor(int shardCount) : HashCodeMessageExtractor(shardCount)
{
    public override string? EntityId(object message)
    {
        return message is IPlayerCommand playerCommand ? playerCommand.UserId : null;
    }
}
