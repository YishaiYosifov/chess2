using System.Text.Json.Serialization;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.PoolKey")]
[method: JsonConstructor]
public record PoolKey(PoolType PoolType, TimeControlSettings TimeControl)
{
    public PoolKey(PoolKeyRequest request)
        : this(request.PoolType, new TimeControlSettings(request.TimeControl)) { }

    public string ToGrainKey() =>
        $"{PoolType.ToString().ToLower()}:{TimeControl.BaseSeconds}+{TimeControl.IncrementSeconds}";

    public static PoolKey FromGrainKey(string key)
    {
        var parts = key.Split(':');
        var poolType = Enum.Parse<PoolType>(parts[0], ignoreCase: true);

        var timeParts = parts[1].Split('+');
        var minutes = int.Parse(timeParts[0]);
        var increment = int.Parse(timeParts[1]);

        return new PoolKey(poolType, new TimeControlSettings(minutes, increment));
    }
}
