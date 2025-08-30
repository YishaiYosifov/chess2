using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.Seeker")]
public record Seeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    DateTimeOffset CreatedAt
)
{
    public virtual bool IsCompatibleWith(Seeker other) =>
        UserId != other.UserId && !BlockedUserIds.Contains(other.UserId);
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.CasualSeeker")]
public record CasualSeeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    DateTimeOffset CreatedAt
) : Seeker(UserId, UserName, BlockedUserIds, CreatedAt);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.RatedSeeker")]
public record RatedSeeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    DateTimeOffset CreatedAt,
    SeekerRating Rating
) : Seeker(UserId, UserName, BlockedUserIds, CreatedAt)
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
[Alias("Chess2.Api.Matchmaking.Models.SeekerRating")]
public record SeekerRating(int Value, int AllowedRatingRange, TimeControl TimeControl)
{
    public int MinRating => Value - AllowedRatingRange;

    public int MaxRating => Value + AllowedRatingRange;

    public bool IsWithinRatingRange(int checkRating) =>
        checkRating >= MinRating && checkRating <= MaxRating;
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.OpenRatedSeeker")]
public record OpenRatedSeeker(
    UserId UserId,
    string UserName,
    HashSet<string> BlockedUserIds,
    DateTimeOffset CreatedAt,
    Dictionary<TimeControl, int> Ratings
) : Seeker(UserId, UserName, BlockedUserIds, CreatedAt)
{
    public override bool IsCompatibleWith(Seeker other) => base.IsCompatibleWith(other);
}
