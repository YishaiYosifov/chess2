using Chess2.Api.Game.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.UserRating.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.RatingTests;

public class RatingServiceTests : BaseIntegrationTest
{
    private readonly IRatingService _ratingService;

    public RatingServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _ratingService = Scope.ServiceProvider.GetRequiredService<IRatingService>();
    }

    [Fact]
    public async Task GetRatingAsync_returns_default_rating_when_none_exists()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var ratingValue = await _ratingService.GetRatingAsync(user, TimeControl.Blitz, CT);

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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(user, timeControl: TimeControl.Rapid)
        );

        var ratingValue = await _ratingService.GetRatingAsync(user, TimeControl.Rapid, CT);

        ratingValue.Should().Be(rating.Value);
    }

    [Fact]
    public async Task UpdateRatingAsync_updates_current_rating_and_adds_archive_entry()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
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
        var whiteUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var blackUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

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

        var newWhiteRating = await _ratingService.GetRatingAsync(whiteUser, TimeControl.Blitz, CT);
        var newBlackRating = await _ratingService.GetRatingAsync(blackUser, TimeControl.Blitz, CT);

        newWhiteRating.Should().Be(whiteRating + expectedWhiteRatingChange);
        newBlackRating.Should().Be(blackRating + expectedBlackRatingChange);
    }
}
