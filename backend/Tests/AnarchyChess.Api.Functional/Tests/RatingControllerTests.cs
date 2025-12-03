using System.Net;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests;

public class RatingControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetRatingArchives_returns_overviews_for_existing_user()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzCurrent = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var blitzArchive = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz).Generate();

        await DbContext.AddRangeAsync(user, blitzCurrent, blitzArchive);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetRatingArchivesAsync(user.Id, since: null);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().ContainSingle();
        var archive = response.Content.Single();
        archive.TimeControl.Should().Be(TimeControl.Blitz);
        archive.Ratings.Should().ContainSingle().Which.Rating.Should().Be(blitzArchive.Value);
    }

    [Fact]
    public async Task GetRatingArchives_with_since_parameter_filters_archives()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzCurrent = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var olderArchive = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz)
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-10))
            .Generate();
        var recentArchive = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz)
            .RuleFor(x => x.AchievedAt, DateTime.UtcNow.AddDays(-1))
            .Generate();

        await DbContext.AddRangeAsync(user, blitzCurrent, olderArchive, recentArchive);
        await DbContext.SaveChangesAsync(CT);

        var since = DateTime.UtcNow.AddDays(-5);
        var response = await ApiClient.Api.GetRatingArchivesAsync(user.Id, since);

        response.IsSuccessful.Should().BeTrue();
        response
            .Content.Should()
            .ContainSingle()
            .Which.Ratings.Should()
            .ContainSingle()
            .Which.Rating.Should()
            .Be(recentArchive.Value);
    }

    [Fact]
    public async Task GetRatingArchives_returns_404_for_nonexistent_user()
    {
        var response = await ApiClient.Api.GetRatingArchivesAsync(
            "nonexistent-user-id",
            since: null
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCurrentRatings_returns_all_current_ratings_for_user()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzRating = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var rapidRating = new CurrentRatingFaker(user, timeControl: TimeControl.Rapid).Generate();

        await DbContext.AddRangeAsync(user, blitzRating, rapidRating);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetCurrentRatingsAsync(user.Id);

        response.IsSuccessful.Should().BeTrue();
        response
            .Content.Should()
            .BeEquivalentTo(
                [
                    new CurrentRatingStatus(TimeControl.Blitz, blitzRating.Value),
                    new CurrentRatingStatus(TimeControl.Rapid, rapidRating.Value),
                ]
            );
    }

    [Fact]
    public async Task GetCurrentRatings_returns_404_for_nonexistent_user()
    {
        var response = await ApiClient.Api.GetCurrentRatingsAsync("nonexistent-user-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
