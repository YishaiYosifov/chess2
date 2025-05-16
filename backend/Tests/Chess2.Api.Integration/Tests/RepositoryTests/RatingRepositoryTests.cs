using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.RepositoryTests;

public class RatingRepositoryTests : BaseIntegrationTest
{
    private readonly IRatingRepository _ratingRepository;

    public RatingRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _ratingRepository = Scope.ServiceProvider.GetRequiredService<IRatingRepository>();
    }

    [Fact]
    public async Task GetAllRatingsAsync_returns_all_rating_for_a_specific_user()
    {
        var otherUser1 = new AuthedUserFaker().Generate();
        var otherUser2 = new AuthedUserFaker().Generate();
        otherUser2.Ratings =
        [
            new RatingFaker(otherUser2).Generate(),
            new RatingFaker(otherUser2).Generate(),
        ];

        var targetUser = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>()
        {
            new RatingFaker(targetUser).Generate(),
            new RatingFaker(targetUser).Generate(),
            new RatingFaker(targetUser).Generate(),
        };
        targetUser.Ratings = ratings;

        await DbContext.AddRangeAsync(otherUser1, otherUser2, targetUser);
        await DbContext.SaveChangesAsync();

        var results = await _ratingRepository.GetAllRatingsAsync(targetUser);

        results.Should().BeEquivalentTo(ratings);
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_finds_the_correct_rating_for_a_time_control()
    {
        var otherUser = new AuthedUserFaker().Generate();
        otherUser.Ratings =
        [
            new RatingFaker(otherUser).Generate(),
            new RatingFaker(otherUser).Generate(),
        ];

        var targetUser = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>()
        {
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Bullet).Generate(),
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Blitz).Generate(),
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Rapid).Generate(),
        };
        var targetRating = ratings[1];
        targetUser.Ratings = ratings;

        await DbContext.AddRangeAsync(otherUser, targetUser);
        await DbContext.SaveChangesAsync();

        var results = await _ratingRepository.GetTimeControlRatingAsync(
            targetUser,
            targetRating.TimeControl
        );

        results.Should().BeEquivalentTo(targetRating);
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_throws_an_exception_when_there_are_duplicate_time_controls()
    {
        var user = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>()
        {
            new RatingFaker(user).RuleFor(x => x.TimeControl, TimeControl.Blitz).Generate(),
            new RatingFaker(user).RuleFor(x => x.TimeControl, TimeControl.Blitz).Generate(),
        };
        user.Ratings = ratings;

        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var act = () => _ratingRepository.GetTimeControlRatingAsync(user, TimeControl.Blitz);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetTimeControlRatingAsync_creates_a_rating_when_it_doesnt_exist()
    {
        var timeControl = TimeControl.Blitz;
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var results = await _ratingRepository.GetTimeControlRatingAsync(user, timeControl);

        results.TimeControl.Should().Be(timeControl);
        results.Value.Should().Be(800);
        results.User.Should().Be(user);

        user.Ratings.Should().HaveCount(1);
        user.Ratings.ElementAt(0).Should().BeEquivalentTo(results);
    }
}
