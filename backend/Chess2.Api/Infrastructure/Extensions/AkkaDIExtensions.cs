using Akka.Cluster.Hosting;
using Akka.Hosting;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Sharding;

namespace Chess2.Api.Infrastructure.Extensions;

public static class AkkaDIExtensions
{
    public static AkkaConfigurationBuilder WithMatchmakingShard<TActor>(this AkkaConfigurationBuilder builder, string name, int shardCount) where TActor : MatchmakingActor
    {
        return builder.WithShardRegion<TActor>(
            name,
            (_, _, resolver) => s => resolver.Props<TActor>(),
            new MatchmakingShardExtractor(shardCount),
            new ShardOptions()
            {
                RememberEntities = true,
                Role = ActorSystemConstants.BackendRole,
            }
        );
    }
}
