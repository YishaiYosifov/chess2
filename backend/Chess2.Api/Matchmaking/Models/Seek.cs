namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.Seek")]
public record Seek(string UserId, string UserName, HashSet<string> BlockedUserIds)
{
    public virtual bool IsCompatibleWith(Seek other) => IsBlockStatusCompatibleWith(other);

    public bool IsBlockStatusCompatibleWith(Seek other) => !BlockedUserIds.Contains(other.UserId);
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.RatedSeek")]
public record RatedSeek(
    string UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    SeekRating Rating
) : Seek(UserId, UserName, BlockedUserIds)
{
    public override bool IsCompatibleWith(Seek other) =>
        base.IsCompatibleWith(other) && IsRatingCompatibleWith(other);

    public bool IsRatingCompatibleWith(Seek other) =>
        other is RatedSeek otherRatedSeek && Rating.IsCompatibleWith(otherRatedSeek.Rating);
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekRating")]
public record SeekRating(int Value, int AllowedRatingRange)
{
    [Id(0)]
    public int MinRating { get; } = Value - AllowedRatingRange;

    [Id(1)]
    public int MaxRating { get; } = Value + AllowedRatingRange;

    public bool IsCompatibleWith(SeekRating other) =>
        IsWithinRatingRange(other.Value) && other.IsWithinRatingRange(Value);

    protected bool IsWithinRatingRange(int checkRating) =>
        checkRating >= MinRating && checkRating <= MaxRating;
}
