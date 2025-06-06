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
    public async Task GetOrCreateRatingAsync_CreatesNewRating_WhenNoneExists()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var result = await _ratingService.GetOrCreateRatingAsync(user, TimeControl.Blitz);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.User.Should().BeEquivalentTo(user);
        Assert.Equal(TimeControl.Blitz, result.TimeControl);
        Assert.Equal(AppSettings.Game.DefaultRating, result.Value);

        var dbRating = await DbContext.Ratings.FirstOrDefaultAsync(
            r => r.UserId == user.Id && r.TimeControl == TimeControl.Blitz,
            CT
        );
        dbRating.Should().NotBeNull();
        dbRating.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task GetOrCreateRatingAsync_ReturnsExistingRating_WhenExists()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(user)
                .RuleFor(r => r.UserId, user.Id)
                .RuleFor(r => r.TimeControl, TimeControl.Rapid)
        );

        var result = await _ratingService.GetOrCreateRatingAsync(user, TimeControl.Rapid);

        rating.Should().NotBeNull();
        rating.Should().BeEquivalentTo(rating);
    }
}
