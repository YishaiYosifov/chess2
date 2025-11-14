using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.RatingTests;

public class CurrentRatingRepositoryTests : BaseIntegrationTest
{
    private readonly ICurrentRatingRepository _ratingRepository;

    public CurrentRatingRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _ratingRepository = Scope.ServiceProvider.GetRequiredService<ICurrentRatingRepository>();
    }

    [Fact]
    public async Task GetRatingAsync_finds_the_correct_rating_for_a_user_and_time_control()
    {
        var userToFind = new AuthedUserFaker().Generate();
        var ratingToFind = new CurrentRatingFaker(
            userToFind,
            timeControl: TimeControl.Blitz
        ).Generate();

        // store a rating for another user to ensure it doesn't interfere
        var otherUser = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(
            userToFind,
            ratingToFind,
            otherUser,
            new CurrentRatingFaker(otherUser, timeControl: ratingToFind.TimeControl).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingRepository.GetRatingAsync(
            userToFind.Id,
            ratingToFind.TimeControl,
            CT
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(ratingToFind);
    }

    [Fact]
    public async Task GetRatingAsync_returns_null_when_the_rating_doesnt_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(
            user,
            new CurrentRatingFaker(user, timeControl: TimeControl.Rapid).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _ratingRepository.GetRatingAsync(user.Id, TimeControl.Blitz, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRatingAsync_adds_the_rating_to_the_user_and_db_context()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var rating = new CurrentRatingFaker(user)
            .RuleFor(x => x.TimeControl, TimeControl.Classical)
            .Generate();

        await _ratingRepository.UpsertRatingAsync(rating, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbRating = await DbContext
            .CurrentRatings.AsNoTracking()
            .SingleOrDefaultAsync(
                r => r.UserId == user.Id && r.TimeControl == TimeControl.Classical,
                CT
            );
        dbRating.Should().NotBeNull();
        dbRating.Should().BeEquivalentTo(rating);
    }
}
