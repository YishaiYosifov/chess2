using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.Seeker")]
public record Seeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    DateTime CreatedAt
)
{
    public virtual bool IsCompatibleWith(Seeker other) => IsBlockStatusCompatibleWith(other);

    public bool IsBlockStatusCompatibleWith(Seeker other) => !BlockedUserIds.Contains(other.UserId);
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.RatedSeeker")]
public record RatedSeeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    SeekerRating Rating,
    DateTime CreatedAt
) : Seeker(UserId, UserName, BlockedUserIds, CreatedAt)
{
    public override bool IsCompatibleWith(Seeker other) =>
        base.IsCompatibleWith(other) && IsRatingCompatibleWith(other);

    public bool IsRatingCompatibleWith(Seeker other) =>
        other is RatedSeeker otherRatedSeek && Rating.IsCompatibleWith(otherRatedSeek.Rating);
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekerRating")]
public record SeekerRating(int Value, int AllowedRatingRange)
{
    [Id(0)]
    public int MinRating { get; } = Value - AllowedRatingRange;

    [Id(1)]
    public int MaxRating { get; } = Value + AllowedRatingRange;

    public bool IsCompatibleWith(SeekerRating other) =>
        IsWithinRatingRange(other.Value) && other.IsWithinRatingRange(Value);

    protected bool IsWithinRatingRange(int checkRating) =>
        checkRating >= MinRating && checkRating <= MaxRating;
}
