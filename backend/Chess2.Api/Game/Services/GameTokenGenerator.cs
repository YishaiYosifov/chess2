using System.Security.Cryptography;
using System.Text;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Chess2.Api.Game.Actors;

namespace Chess2.Api.Game.Services;

public interface IGameTokenGenerator
{
    Task<string> GenerateUniqueGameToken();
}

public class GameTokenGenerator(IRequiredActor<GameActor> gameActor) : IGameTokenGenerator
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;

    private const string TokenCharSet =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public async Task<string> GenerateUniqueGameToken()
    {
        var shardState = await _gameActor.ActorRef.Ask<CurrentShardRegionState>(
            GetShardRegionState.Instance
        );
        var existingTokenActors = shardState
            .Shards.SelectMany(shard => shard.EntityIds)
            .ToHashSet();

        while (true)
        {
            var token = GenerateBase62Token(16);
            if (!existingTokenActors.Contains(token))
                return token;
        }
    }

    private static string GenerateBase62Token(int length = 16)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            // Map byte to 0-61 range (Base62)
            result.Append(TokenCharSet[b % TokenCharSet.Length]);
        }

        return result.ToString();
    }
}
