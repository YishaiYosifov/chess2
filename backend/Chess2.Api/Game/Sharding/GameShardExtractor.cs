using Akka.Cluster.Sharding;
using Chess2.Api.Game.Models;

namespace Chess2.Api.Game.Sharding;

public class GameShardExtractor(int shardCount) : HashCodeMessageExtractor(shardCount)
{
    public override string? EntityId(object message) =>
        message is IGameMessage gameMessage ? gameMessage.GameToken : null;
}
