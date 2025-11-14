using System.Collections.Immutable;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.TimeControlSettings")]
public readonly record struct TimeControlSettings(int BaseSeconds, int IncrementSeconds)
{
    public static readonly ImmutableHashSet<int> AllowedBaseSeconds =
    [
        15,
        30,
        60,
        120,
        180,
        300,
        420,
        600,
        900,
        1200,
        1500,
        1800,
        2700,
        3600,
        5400,
    ];

    public static readonly ImmutableHashSet<int> AllowedIncrementSeconds =
    [
        0,
        1,
        2,
        3,
        4,
        5,
        10,
        15,
        20,
        25,
        30,
        60,
    ];
}
