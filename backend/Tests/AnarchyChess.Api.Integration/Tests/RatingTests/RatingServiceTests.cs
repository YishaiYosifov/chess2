using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Models;
using AnarchyChess.Api.UserRating.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.RatingTests;

public class RatingServiceTests : BaseIntegrationTest
{
    private readonly IRatingService _ratingService;

    public RatingServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _ratingService = Scope.ServiceProvider.GetRequiredService<IRatingService>();
    }

    [Fact]
    public async Task GetRatingAsync_returns_default_rating_when_none_exists()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var ratingValue = await _ratingService.GetRatingAsync(user.Id, TimeControl.Blitz, CT);

        ratingValue.Should().Be(AppSettings.Game.DefaultRating);

        var dbRating = await DbContext
            .CurrentRatings.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.UserId == user.Id && r.TimeControl == TimeControl.Blitz,
                CT
            );
        dbRating.Should().BeNull();
    }

    [Fact]
    public async Task GetRatingAsync_returns_existing_rating()
    {
        var user = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(user, timeControl: TimeControl.Rapid).Generate();
        await DbContext.AddRangeAsync(user, rating);
        await DbContext.SaveChangesAsync(CT);

        var ratingValue = await _ratingService.GetRatingAsync(user.Id, TimeControl.Rapid, CT);

        ratingValue.Should().Be(rating.Value);
    }

    [Fact]
    public async Task UpdateRatingAsync_updates_current_rating_and_adds_archive_entry()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);
        int newRatingValue = 1500;
        var timeControl = TimeControl.Blitz;

        await _ratingService.UpdateRatingAsync(user, timeControl, newRatingValue, CT);
        await DbContext.SaveChangesAsync(CT);

        var current = await DbContext
            .CurrentRatings.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.TimeControl == timeControl, CT);

        current.Should().NotBeNull();
        current.Value.Should().Be(newRatingValue);
        current.TimeControl.Should().Be(timeControl);
        current.UserId.Should().Be(user.Id);

        var archives = await DbContext
            .RatingArchives.AsNoTracking()
            .Where(r => r.UserId == user.Id && r.TimeControl == timeControl)
            .ToListAsync(CT);

        archives.Should().ContainSingle();
        var archive = archives.Single();
        archive.Value.Should().Be(newRatingValue);
        archive.TimeControl.Should().Be(timeControl);
        archive.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateRatingAsync_can_be_called_multiple_times_and_appends_archives()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);
        var timeControl = TimeControl.Classical;

        int[] ratingSequence = [1400, 1500, 1600];
        foreach (var rating in ratingSequence)
        {
            await _ratingService.UpdateRatingAsync(user, timeControl, rating, CT);
            await DbContext.SaveChangesAsync(CT);
        }

        // Assert current rating is the last one
        var current = await DbContext
            .CurrentRatings.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.TimeControl == timeControl, CT);

        current.Should().NotBeNull();
        current!.Value.Should().Be(ratingSequence.Last());

        // Assert an archive entry exists for each update
        var archives = await DbContext
            .RatingArchives.AsNoTracking()
            .Where(r => r.UserId == user.Id && r.TimeControl == timeControl)
            .OrderBy(r => r.Id)
            .ToListAsync(CT);

        archives.Should().HaveCount(ratingSequence.Length);
        archives
            .Select(a => a.Value)
            .Should()
            .BeEquivalentTo(ratingSequence, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GetRatingOverviewsAsync_returns_overviews_only_for_controls_with_current_ratings()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzCurrent = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();

        var blitzHigh = new RatingArchiveFaker(
            user,
            rating: blitzCurrent.Value + 100,
            timeControl: TimeControl.Blitz
        )
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-30))
            .Generate();
        var blitzWithin1 = new RatingArchiveFaker(
            user,
            rating: blitzCurrent.Value + 10,
            timeControl: TimeControl.Blitz
        )
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-3))
            .Generate();
        var blitzWithin2 = new RatingArchiveFaker(
            user,
            rating: blitzCurrent.Value - 10,
            TimeControl.Blitz
        )
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-1))
            .Generate();
        var blitzLow = new RatingArchiveFaker(
            user,
            rating: blitzCurrent.Value - 100,
            timeControl: TimeControl.Blitz
        )
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-20))
            .Generate();

        var otherUser = new AuthedUserFaker().Generate();
        var otherUserRating = new CurrentRatingFaker(
            otherUser,
            timeControl: TimeControl.Blitz
        ).Generate();

        await DbContext.AddRangeAsync(
            user,
            blitzCurrent,
            blitzHigh,
            blitzWithin1,
            blitzWithin2,
            blitzLow,
            otherUser,
            otherUserRating
        );
        await DbContext.SaveChangesAsync(CT);

        var since = DateTime.UtcNow.AddDays(-7);

        var result = await _ratingService.GetRatingOverviewsAsync(user, since, CT);

        // only blitz has current
        RatingOverview expectedBlitzOverview = new(
            TimeControl.Blitz,
            Current: blitzCurrent.Value,
            Highest: blitzHigh.Value,
            Lowest: blitzLow.Value,
            Ratings: [new RatingSummary(blitzWithin1), new RatingSummary(blitzWithin2)]
        );
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedBlitzOverview);
    }

    [Fact]
    public async Task GetRatingOverviewsAsync_returns_empty_when_user_has_no_current_ratings()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingService.GetRatingOverviewsAsync(user, since: null, CT);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRatingOverviewsAsync_returns_ratings_with_archives_out_of_date_range()
    {
        var user = new AuthedUserFaker().Generate();
        var currentRating = new CurrentRatingFaker(user).Generate();
        var oldRating = new RatingArchiveFaker(user, timeControl: currentRating.TimeControl)
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-10))
            .Generate();

        await DbContext.AddRangeAsync(user, currentRating, oldRating);
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingService.GetRatingOverviewsAsync(user, DateTime.UtcNow, CT);

        result.Should().ContainSingle();
        var overview = result.Single();
        overview.Current.Should().Be(currentRating.Value);
        overview.Ratings.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRatingOverviewsAsync_with_null_since_returns_all_archives()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzCurrent = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();

        var older = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz)
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddYears(-2))
            .Generate();
        var recent = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz)
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-1))
            .Generate();
        await DbContext.AddRangeAsync(user, blitzCurrent, older, recent);
        await DbContext.SaveChangesAsync(CT);

        var overviews = await _ratingService.GetRatingOverviewsAsync(user, since: null, CT);

        overviews.Should().ContainSingle();
        overviews
            .ElementAt(0)
            .Ratings.Select(r => r.AchievedAt)
            .Should()
            .BeEquivalentTo([older.AchievedAt, recent.AchievedAt]);
    }

    [Fact]
    public async Task GetCurrentRatingsAsync_returns_all_current_ratings_for_user()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzRating = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var rapidRating = new CurrentRatingFaker(user, timeControl: TimeControl.Rapid).Generate();
        var anotherUser = new AuthedUserFaker().Generate();
        var anotherUserRating = new CurrentRatingFaker(
            anotherUser,
            timeControl: TimeControl.Bullet
        ).Generate();

        await DbContext.AddRangeAsync(
            user,
            blitzRating,
            rapidRating,
            anotherUser,
            anotherUserRating
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingService.GetCurrentRatingsAsync(user, CT);

        result
            .Should()
            .BeEquivalentTo(
                [
                    new CurrentRatingStatus(
                        TimeControl: TimeControl.Blitz,
                        Rating: blitzRating.Value
                    ),
                    new CurrentRatingStatus(
                        TimeControl: TimeControl.Rapid,
                        Rating: rapidRating.Value
                    ),
                ]
            );
    }

    [Theory]
    [InlineData(GameResult.WhiteWin, 1500, 1700, 12, -12)]
    [InlineData(GameResult.BlackWin, 1500, 1700, -4, 4)]
    [InlineData(GameResult.Draw, 1500, 1700, 4, -4)]
    // rating can't go bellow 100
    [InlineData(GameResult.WhiteWin, 100, 100, 8, 0)]
    [InlineData(GameResult.BlackWin, 100, 100, 0, 8)]
    public async Task UpdateRatingForResultAsync_updates_ratings_correctly(
        GameResult gameResult,
        int whiteRating,
        int blackRating,
        int expectedWhiteRatingChange,
        int expectedBlackRatingChange
    )
    {
        var whiteUser = new AuthedUserFaker().Generate();
        var blackUser = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(whiteUser, blackUser);
        await DbContext.SaveChangesAsync(CT);

        await _ratingService.UpdateRatingAsync(whiteUser, TimeControl.Blitz, whiteRating, CT);
        await _ratingService.UpdateRatingAsync(blackUser, TimeControl.Blitz, blackRating, CT);
        await DbContext.SaveChangesAsync(CT);

        await _ratingService.UpdateRatingForResultAsync(
            whiteUser,
            blackUser,
            gameResult,
            TimeControl.Blitz,
            CT
        );
        await DbContext.SaveChangesAsync(CT);

        var newWhiteRating = await _ratingService.GetRatingAsync(
            whiteUser.Id,
            TimeControl.Blitz,
            CT
        );
        var newBlackRating = await _ratingService.GetRatingAsync(
            blackUser.Id,
            TimeControl.Blitz,
            CT
        );

        newWhiteRating.Should().Be(whiteRating + expectedWhiteRatingChange);
        newBlackRating.Should().Be(blackRating + expectedBlackRatingChange);
    }
}
