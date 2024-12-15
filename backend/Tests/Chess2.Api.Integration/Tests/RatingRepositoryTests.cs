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
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        user2.Ratings = [
            new RatingFaker(user2).Generate(),
            new RatingFaker(user2).Generate(),
        ];

        var getFromUser = new AuthedUserFaker().Generate();
        var ratings = new List<Rating>() {
            new RatingFaker(getFromUser).Generate(),
            new RatingFaker(getFromUser).Generate(),
            new RatingFaker(getFromUser).Generate()
        };
        getFromUser.Ratings = ratings;

        await DbContext.AddRangeAsync(user1, user2, getFromUser);
        await DbContext.SaveChangesAsync();

        var results = await _ratingRepository.GetAllRatings(getFromUser);

        results.Should().BeEquivalentTo(ratings);
    }
}
