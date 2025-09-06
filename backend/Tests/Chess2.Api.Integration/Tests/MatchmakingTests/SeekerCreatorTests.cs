﻿using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
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
    private readonly LobbySettings _lobbySettings;
    private readonly GameSettings _gameSettings;

    public SeekerCreatorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var ratingService = Scope.ServiceProvider.GetRequiredService<IRatingService>();
        var blockService = Scope.ServiceProvider.GetRequiredService<IBlockService>();
        var timeControlTranslator =
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>();
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _timeProvicerMock.GetUtcNow().Returns(_fakeNow);

        _lobbySettings = settings.Value.Lobby;
        _gameSettings = settings.Value.Game;
        _seekerCreator = new SeekerCreator(
            ratingService,
            timeControlTranslator,
            settings,
            blockService,
            _timeProvicerMock
        );
    }

    [Fact]
    public async Task RatedSeekerAsync_retuns_returns_expected_rated_seeker()
    {
        var user = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var blockedUsers = new BlockedUserFaker(user.Id).Generate(3);
        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.AddRangeAsync(user, rating);
        await DbContext.SaveChangesAsync(CT);

        var timeControl = new TimeControlSettings { BaseSeconds = 300 };
        var seeker = await _seekerCreator.CreateRatedSeekerAsync(user, timeControl, CT);

        SeekerRating expectedRating = new(
            Value: rating.Value,
            AllowedRatingRange: _lobbySettings.AllowedMatchRatingDifference,
            TimeControl: TimeControl.Blitz
        );
        RatedSeeker expectedSeeker = new(
            UserId: user.Id,
            UserName: user.UserName!,
            BlockedUserIds: [.. blockedUsers.Select(b => b.BlockedUserId)],
            Rating: expectedRating,
            CreatedAt: _fakeNow
        );

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }

    [Fact]
    public async Task CreateAuthedCasualSeeker_returns_expected_seeker()
    {
        var user = new AuthedUserFaker().Generate();
        var blockedUsers = new BlockedUserFaker(user.Id).Generate(3);
        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        CasualSeeker expectedSeeker = new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [.. blockedUsers.Select(b => b.BlockedUserId)],
            CreatedAt: _fakeNow
        );

        var seeker = await _seekerCreator.CreateAuthedCasualSeekerAsync(user, CT);

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }

    [Fact]
    public void CreateGuestCasualSeeker_returns_expected_seeker()
    {
        var userId = Guid.NewGuid().ToString();

        CasualSeeker expectedSeeker = new(
            UserId: userId,
            UserName: "Guest",
            BlockedUserIds: [],
            CreatedAt: _fakeNow
        );

        var seeker = _seekerCreator.CreateGuestCasualSeeker(userId);

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }

    [Fact]
    public async Task CreateRatedOpenSeekerAsync_returns_expected_open_seeker()
    {
        var user = new AuthedUserFaker().Generate();
        var blitzRating = new CurrentRatingFaker(user, timeControl: TimeControl.Blitz).Generate();
        var classicalRating = new CurrentRatingFaker(
            user,
            timeControl: TimeControl.Classical
        ).Generate();
        var blockedUsers = new BlockedUserFaker(user.Id).Generate(3);
        await DbContext.AddRangeAsync(user, blitzRating, classicalRating);
        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        var seeker = await _seekerCreator.CreateRatedOpenSeekerAsync(user, CT);

        Dictionary<TimeControl, int> expectedRatings = new()
        {
            [TimeControl.Bullet] = _gameSettings.DefaultRating,
            [TimeControl.Blitz] = blitzRating.Value,
            [TimeControl.Rapid] = _gameSettings.DefaultRating,
            [TimeControl.Classical] = classicalRating.Value,
        };

        OpenRatedSeeker expectedSeeker = new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [.. blockedUsers.Select(b => b.BlockedUserId)],
            Ratings: expectedRatings,
            CreatedAt: _fakeNow
        );

        seeker.Should().BeEquivalentTo(expectedSeeker);
    }
}
