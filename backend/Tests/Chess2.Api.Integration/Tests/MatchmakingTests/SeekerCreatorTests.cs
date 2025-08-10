using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.MatchmakingTests;

public class SeekerCreatorTests : BaseIntegrationTest
{
    private readonly TimeProvider _timeProvicerMock = Substitute.For<TimeProvider>();

    private readonly DateTime _fakeNow = DateTime.UtcNow;
    private readonly SeekerCreator _seekerCreator;
    private readonly GameSettings _settings;

    public SeekerCreatorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var ratingService = Scope.ServiceProvider.GetRequiredService<IRatingService>();
        var timeControlTranslator =
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>();
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _timeProvicerMock.GetUtcNow().Returns(_fakeNow);

        _settings = settings.Value.Game;
        _seekerCreator = new SeekerCreator(
            ratingService,
            timeControlTranslator,
            settings,
            _timeProvicerMock
        );
    }

    [Fact]
    public async Task RatedSeekerAsync_retuns_returns_expected_rated_seeker()
    {
        var user = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(user).Generate();
        await DbContext.AddRangeAsync(user, rating);
        await DbContext.SaveChangesAsync(CT);

        var timeControl = new TimeControlSettings { BaseSeconds = 300 };

        var seeker = await _seekerCreator.RatedSeekerAsync(user, timeControl);

        SeekerRating expectedRating = new(
            Value: rating.Value,
            AllowedRatingRange: _settings.AllowedMatchRatingDifference
        );
        RatedSeeker expectedSeeker = new(
            UserId: user.Id,
            UserName: user.UserName!,
            BlockedUserIds: [],
            Rating: expectedRating,
            CreatedAt: _fakeNow
        );
        seeker.Should().BeEquivalentTo(expectedSeeker);
    }

    [Fact]
    public async Task CasualSeeker_with_a_user_returns_expected_seeker()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var expectedSeeker = new Seeker(
            UserId: user.Id,
            UserName: user.UserName!,
            BlockedUserIds: [],
            CreatedAt: _fakeNow
        );

        var seeker = _seekerCreator.CasualSeeker(user);

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }

    [Fact]
    public void CasualSeeker_with_a_guest_returns_expected_seeker()
    {
        var userId = Guid.NewGuid().ToString();

        var expectedSeeker = new Seeker(
            UserId: userId,
            UserName: "Guest",
            BlockedUserIds: [],
            CreatedAt: _fakeNow
        );

        var seeker = _seekerCreator.CasualSeeker(userId);

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }
}
