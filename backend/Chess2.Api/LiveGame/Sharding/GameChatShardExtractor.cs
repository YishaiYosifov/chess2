using Akka.Cluster.Sharding;
using Chess2.Api.LiveGame.Models;

namespace Chess2.Api.LiveGame.Sharding;

public class GameChatShardExtractor(int shardCount) : HashCodeMessageExtractor(shardCount)
{
    public override string? EntityId(object message) =>
        message is IGameChatMessage gameChat ? gameChat.GameToken : null;
}
