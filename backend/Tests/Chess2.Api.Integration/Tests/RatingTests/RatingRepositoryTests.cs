using Chess2.Api.Game.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.UserRating.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.RatingTests;

public class RatingRepositoryTests : BaseIntegrationTest
{
    private readonly IRatingRepository _ratingRepository;

    public RatingRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _ratingRepository = Scope.ServiceProvider.GetRequiredService<IRatingRepository>();
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_finds_the_correct_rating_for_a_user_and_time_control()
    {
        var userToFind = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var ratingToFind = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(userToFind, timeControl: TimeControl.Blitz)
        );
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(userToFind, timeControl: TimeControl.Classical)
        );

        // store a rating for another user to ensure it doesn't interfere
        var otherUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(otherUser, timeControl: ratingToFind.TimeControl)
        );

        var result = await _ratingRepository.GetTimeControlRatingAsync(
            userToFind,
            ratingToFind.TimeControl
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(ratingToFind);
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_returns_null_when_the_rating_doesnt_exist()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(user, timeControl: TimeControl.Rapid)
        );

        var result = await _ratingRepository.GetTimeControlRatingAsync(user, TimeControl.Blitz);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_finds_the_latest_rating_when_there_are_multiple()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var ratings = new RatingFaker(user, timeControl: TimeControl.Rapid).Generate(3);
        await DbContext.Ratings.AddRangeAsync(ratings, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingRepository.GetTimeControlRatingAsync(user, TimeControl.Rapid);

        result.Should().BeEquivalentTo(ratings.Last());
    }

    [Fact]
    public async Task AddRatingAsync_adds_the_rating_to_the_user_and_db_context()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var rating = new RatingFaker(user)
            .RuleFor(x => x.TimeControl, TimeControl.Classical)
            .Generate();

        await _ratingRepository.AddRatingAsync(rating, user);
        await DbContext.SaveChangesAsync(CT);

        var dbRating = await DbContext.Ratings.SingleOrDefaultAsync(
            r => r.UserId == user.Id && r.TimeControl == TimeControl.Classical,
            CT
        );
        dbRating.Should().NotBeNull();
        dbRating.Should().BeEquivalentTo(rating);
        user.Ratings.Should().Contain(dbRating);
    }
}
