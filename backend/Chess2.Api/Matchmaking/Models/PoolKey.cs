using System.Text.Json;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Matchmaking.Models;

public record PoolKey(PoolType PoolType, TimeControlSettings TimeControl)
{
    public override string ToString() => JsonSerializer.Serialize(this);

    public static PoolKey Parse(string raw) =>
        JsonSerializer.Deserialize<PoolKey>(raw)
        ?? throw new FormatException("Invalid PoolKey JSON");
}
