using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.Seeker")]
public record Seeker(
    UserId UserId,
    string UserName,
    HashSet<UserId> ExcludeUserIds,
    DateTimeOffset CreatedAt
)
{
    public virtual bool IsCompatibleWith(Seeker other) =>
        UserId != other.UserId && !ExcludeUserIds.Contains(other.UserId);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.CasualSeeker")]
public record CasualSeeker(
    UserId UserId,
    string UserName,
    HashSet<UserId> ExcludeUserIds,
    DateTimeOffset CreatedAt
) : Seeker(UserId, UserName, ExcludeUserIds, CreatedAt);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.RatedSeeker")]
public record RatedSeeker(
    UserId UserId,
    string UserName,
    HashSet<UserId> ExcludeUserIds,
    DateTimeOffset CreatedAt,
    SeekerRating Rating
) : Seeker(UserId, UserName, ExcludeUserIds, CreatedAt)
{
    public override bool IsCompatibleWith(Seeker other)
    {
        if (!base.IsCompatibleWith(other))
            return false;

        return other switch
        {
            RatedSeeker otherRatedSeeker => IsRatingCompatibleWith(otherRatedSeeker),
            OpenRatedSeeker openRatedSeeker => IsRatingCompatibleWith(openRatedSeeker),
            _ => false,
        };
    }

    public bool IsRatingCompatibleWith(RatedSeeker other) =>
        Rating.IsWithinRatingRange(other.Rating.Value);

    public bool IsRatingCompatibleWith(OpenRatedSeeker other) =>
        other.Ratings.TryGetValue(Rating.TimeControl, out var ratingValue)
        && Rating.IsWithinRatingRange(ratingValue);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.SeekerRating")]
public record SeekerRating(int Value, int? AllowedRatingRange, TimeControl TimeControl)
{
    public int? MinRating => Value - AllowedRatingRange;

    public int? MaxRating => Value + AllowedRatingRange;

    public bool IsWithinRatingRange(int checkRating) =>
        AllowedRatingRange is null || (checkRating >= MinRating && checkRating <= MaxRating);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.OpenRatedSeeker")]
public record OpenRatedSeeker(
    UserId UserId,
    string UserName,
    HashSet<UserId> ExcludeUserIds,
    DateTimeOffset CreatedAt,
    Dictionary<TimeControl, int> Ratings
) : Seeker(UserId, UserName, ExcludeUserIds, CreatedAt)
{
    public override bool IsCompatibleWith(Seeker other) => base.IsCompatibleWith(other);
}
