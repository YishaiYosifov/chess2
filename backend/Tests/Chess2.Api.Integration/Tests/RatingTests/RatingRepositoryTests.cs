﻿using Chess2.Api.Game.Models;
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
    private readonly ICurrentRatingRepository _ratingRepository;

    public RatingRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _ratingRepository = Scope.ServiceProvider.GetRequiredService<ICurrentRatingRepository>();
    }

    [Fact]
    public async Task GetRatingAsync_finds_the_correct_rating_for_a_user_and_time_control()
    {
        var userToFind = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var ratingToFind = await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(userToFind, timeControl: TimeControl.Blitz)
        );

        // store a rating for another user to ensure it doesn't interfere
        var otherUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(otherUser, timeControl: ratingToFind.TimeControl)
        );

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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(user, timeControl: TimeControl.Rapid)
        );

        var result = await _ratingRepository.GetRatingAsync(user.Id, TimeControl.Blitz, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRatingAsync_adds_the_rating_to_the_user_and_db_context()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

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
