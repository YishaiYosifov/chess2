using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class SeekerTests
{
    [Fact]
    public void Seeker_IsCompatibleWith_rejects_seekers_of_the_same_user_id()
    {
        UserId userId = "user1";
        Seeker seeker1 = new(userId, "username1", [], DateTimeOffset.UtcNow);
        Seeker seeker2 = new(userId, "username1", [], DateTimeOffset.UtcNow);

        seeker1.IsCompatibleWith(seeker2).Should().BeFalse();
    }

    [Fact]
    public void Seeker_IsCompatibleWith_rejects_blocked_users()
    {
        Seeker seeker1 = new("user1", "username1", ["user2"], DateTimeOffset.UtcNow);
        Seeker seeker2 = new("user2", "username2", [], DateTimeOffset.UtcNow);

        seeker1.IsCompatibleWith(seeker2).Should().BeFalse();
    }

    [Fact]
    public void Seeker_IsCompatibleWith_allows_unblocked_users()
    {
        Seeker seeker1 = new("user1", "username1", [], DateTimeOffset.UtcNow);
        Seeker seeker2 = new("user2", "username2", [], DateTimeOffset.UtcNow);

        seeker1.IsCompatibleWith(seeker2).Should().BeTrue();
    }

    [Fact]
    public void RatedSeeker_IsCompatibleWith_respects_rating_range()
    {
        RatedSeeker seeker1 = new(
            "user1",
            "username1",
            [],
            DateTimeOffset.UtcNow,
            new SeekerRating(1500, 100, TimeControl.Blitz)
        );

        RatedSeeker seeker2 = new(
            "user2",
            "username2",
            [],
            DateTimeOffset.UtcNow,
            new SeekerRating(1550, 50, TimeControl.Blitz)
        );

        RatedSeeker seeker3 = new(
            "user3",
            "username3",
            [],
            DateTimeOffset.UtcNow,
            new SeekerRating(1700, 50, TimeControl.Blitz)
        );

        seeker1.IsCompatibleWith(seeker2).Should().BeTrue();
        seeker1.IsCompatibleWith(seeker3).Should().BeFalse();
    }

    [Fact]
    public void RatedSeeker_IsCompatibleWith_uses_the_correct_time_control_with_OpenRatedSeeker()
    {
        RatedSeeker ratedSeeker = new(
            "user1",
            "username1",
            [],
            DateTimeOffset.UtcNow,
            new SeekerRating(1500, 100, TimeControl.Rapid)
        );

        OpenRatedSeeker openSeeker = new(
            "user2",
            "username2",
            [],
            DateTimeOffset.UtcNow,
            new Dictionary<TimeControl, int> { { TimeControl.Rapid, 1550 } }
        );

        ratedSeeker.IsCompatibleWith(openSeeker).Should().BeTrue();

        OpenRatedSeeker incompatibleOpenSeeker = new(
            "user3",
            "username3",
            [],
            DateTimeOffset.UtcNow,
            new Dictionary<TimeControl, int> { { TimeControl.Rapid, 1700 } }
        );

        ratedSeeker.IsCompatibleWith(incompatibleOpenSeeker).Should().BeFalse();
    }

    [Fact]
    public void RatedSeeker_IsCompatibleWith_rejects_CasualSeeker()
    {
        RatedSeeker ratedSeeker = new(
            "user1",
            "username1",
            [],
            DateTimeOffset.UtcNow,
            new SeekerRating(1500, 100, TimeControl.Rapid)
        );
        CasualSeeker casualSeeker = new("user2", "username2", [], DateTimeOffset.UtcNow);

        ratedSeeker.IsCompatibleWith(casualSeeker).Should().BeFalse();
    }

    [Fact]
    public void SeekerRating_IsWithinRatingRange_checks_rating_range_correctly()
    {
        var rating = new SeekerRating(1500, 100, TimeControl.Blitz);

        rating.IsWithinRatingRange(1500).Should().BeTrue();
        rating.IsWithinRatingRange(1400).Should().BeTrue();
        rating.IsWithinRatingRange(1600).Should().BeTrue();
        rating.IsWithinRatingRange(1399).Should().BeFalse();
        rating.IsWithinRatingRange(1601).Should().BeFalse();
    }
}
