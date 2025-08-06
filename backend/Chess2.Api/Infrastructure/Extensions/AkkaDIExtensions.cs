using Akka.Cluster.Hosting;
using Akka.Hosting;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Sharding;

namespace Chess2.Api.Infrastructure.Extensions;

public static class AkkaDIExtensions
{
    public static AkkaConfigurationBuilder WithGameShard(
        this AkkaConfigurationBuilder builder,
        int shardCount
    )
    {
        return builder.WithShardRegion<GameActor>(
            "game",
            (_, _, resolver) => s => resolver.Props<GameActor>(s),
            new GameShardExtractor(shardCount),
            new ShardOptions()
            {
                Role = ActorSystemConstants.BackendRole,
                ShouldPassivateIdleEntities = false,
            }
        );
    }
}
