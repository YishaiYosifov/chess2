using Akka.Cluster.Hosting;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Sharding;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Sharding;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.PlayerSession.Sharding;

namespace Chess2.Api.Infrastructure.Extensions;

public static class AkkaDIExtensions
{
    public static AkkaConfigurationBuilder WithMatchmakingShard<TActor>(
        this AkkaConfigurationBuilder builder,
        string name,
        int shardCount
    )
        where TActor : MatchmakingActor
    {
        return builder.WithShardRegion<TActor>(
            name,
            (_, _, resolver) => s => resolver.Props<TActor>(s),
            new MatchmakingShardExtractor(shardCount),
            new ShardOptions()
            {
                Role = ActorSystemConstants.BackendRole,
                ShouldPassivateIdleEntities = false,
            }
        );
    }

    public static AkkaConfigurationBuilder WithPlayerShard(
        this AkkaConfigurationBuilder builder,
        int shardCount
    )
    {
        return builder.WithShardRegion<PlayerSessionActor>(
            "player",
            (_, _, resolver) => s => resolver.Props<PlayerSessionActor>(s),
            new PlayerSessionShardExtractor(shardCount),
            new ShardOptions()
            {
                Role = ActorSystemConstants.BackendRole,
                ShouldPassivateIdleEntities = false,
            }
        );
    }

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
