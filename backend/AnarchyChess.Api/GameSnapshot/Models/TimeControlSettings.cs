using System.Text.Json.Serialization;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.TimeControlSettings")]
[method: JsonConstructor]
public record TimeControlSettings(int BaseSeconds, int IncrementSeconds)
{
    public TimeControlSettings(TimeControlSettingsRequest request)
        : this(request.BaseSeconds, request.IncrementSeconds) { }

    public TimeControl Type =>
        BaseSeconds switch
        {
            < 180 => TimeControl.Bullet, // less than 3 minutes
            <= 300 => TimeControl.Blitz, // 5 minutes or less
            <= 1200 => TimeControl.Rapid, // 20 minutes or less
            _ => TimeControl.Classical,
        };
}
