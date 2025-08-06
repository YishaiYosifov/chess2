namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.TimeControlSettings")]
public readonly record struct TimeControlSettings(int BaseSeconds, int IncrementSeconds);
