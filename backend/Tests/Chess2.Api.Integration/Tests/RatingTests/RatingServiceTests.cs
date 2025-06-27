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
    public async Task GetOrCreateRatingAsync_creates_a_new_rating_when_one_doesnt_exist()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var result = await _ratingService.GetOrCreateRatingAsync(user, TimeControl.Blitz, CT);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.TimeControl.Should().Be(TimeControl.Blitz);
        result.Value.Should().Be(AppSettings.Game.DefaultRating);

        var dbRating = await DbContext
            .Ratings.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.UserId == user.Id && r.TimeControl == TimeControl.Blitz,
                CT
            );
        dbRating.Should().NotBeNull();
        dbRating.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task GetOrCreateRatingAsync_finds_the_existing_rating()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(user, timeControl: TimeControl.Rapid)
        );

        var result = await _ratingService.GetOrCreateRatingAsync(user, TimeControl.Rapid, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(rating);
    }

    [Fact]
    public async Task AddRatingAsync_adds_a_new_rating()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        int newRatingValue = 1500;

        var result = await _ratingService.AddRatingAsync(
            user,
            TimeControl.Classical,
            newRatingValue,
            CT
        );
        await DbContext.SaveChangesAsync(CT);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.TimeControl.Should().Be(TimeControl.Classical);
        result.Value.Should().Be(newRatingValue);

        var dbRating = await DbContext
            .Ratings.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.UserId == user.Id && r.TimeControl == TimeControl.Classical,
                CT
            );
        dbRating.Should().NotBeNull();
        dbRating.Should().BeEquivalentTo(result);
    }

    [Theory]
    [InlineData(GameResult.WhiteWin, 24, -24)]
    [InlineData(GameResult.BlackWin, -8, 8)]
    [InlineData(GameResult.Draw, 8, -8)]
    public async Task UpdateRatingForResultAsync_updates_ratings_correctly(
        GameResult gameResult,
        int expectedWhiteRatingDelta,
        int expectedBlackRatingDelta
    )
    {
        var whiteUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var blackUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        const int whiteRating = 1500;
        const int blackRating = 1700;
        await _ratingService.AddRatingAsync(whiteUser, TimeControl.Blitz, whiteRating, CT);
        await _ratingService.AddRatingAsync(blackUser, TimeControl.Blitz, blackRating, CT);
        await DbContext.SaveChangesAsync(CT);

        await _ratingService.UpdateRatingForResultAsync(
            whiteUser,
            blackUser,
            gameResult,
            TimeControl.Blitz,
            CT
        );
        await DbContext.SaveChangesAsync(CT);

        var newWhiteRating = await _ratingService.GetOrCreateRatingAsync(
            whiteUser,
            TimeControl.Blitz,
            CT
        );
        var newBlackRating = await _ratingService.GetOrCreateRatingAsync(
            blackUser,
            TimeControl.Blitz,
            CT
        );

        newWhiteRating.Value.Should().Be(whiteRating + expectedWhiteRatingDelta);
        newBlackRating.Value.Should().Be(blackRating + expectedBlackRatingDelta);
    }
}
