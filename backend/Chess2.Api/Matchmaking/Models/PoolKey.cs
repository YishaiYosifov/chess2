using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.PoolKey")]
public record PoolKey(PoolType PoolType, TimeControlSettings TimeControl)
{
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
