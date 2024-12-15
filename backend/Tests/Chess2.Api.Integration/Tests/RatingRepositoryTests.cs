using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Tests;

public class RatingRepositoryTests : BaseIntegrationTest
{
    private readonly IRatingRepository _ratingRepository;

    public RatingRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _ratingRepository = Scope.ServiceProvider.GetRequiredService<IRatingRepository>();
    }

    [Fact]
    public async Task Get_all_ratings_for_specific_user()
    {
        var otherUser1 = new AuthedUserFaker().Generate();
        var otherUser2 = new AuthedUserFaker().Generate();
        otherUser2.Ratings = [
            new RatingFaker(otherUser2).Generate(),
            new RatingFaker(otherUser2).Generate(),
        ];

        var targetUser = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>() {
            new RatingFaker(targetUser).Generate(),
            new RatingFaker(targetUser).Generate(),
            new RatingFaker(targetUser).Generate()
        };
        targetUser.Ratings = ratings;

        await DbContext.AddRangeAsync(otherUser1, otherUser2, targetUser);
        await DbContext.SaveChangesAsync();

        var results = await _ratingRepository.GetAllRatings(targetUser);

        results.Should().BeEquivalentTo(ratings);
    }

    [Fact]
    public async Task Get_rating_for_time_control()
    {
        var otherUser = new AuthedUserFaker().Generate();
        otherUser.Ratings = [
            new RatingFaker(otherUser).Generate(),
            new RatingFaker(otherUser).Generate(),
        ];

        var targetUser = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>() {
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Bullet).Generate(),
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Blitz).Generate(),
            new RatingFaker(targetUser).RuleFor(x => x.TimeControl, TimeControl.Rapid).Generate()
        };
        var targetRating = ratings[1];
        targetUser.Ratings = ratings;

        await DbContext.AddRangeAsync(otherUser, targetUser);
        await DbContext.SaveChangesAsync();

        var results = await _ratingRepository.GetTimeControlRating(targetUser, targetRating.TimeControl);

        results.Should().BeEquivalentTo(targetRating);
    }
}
