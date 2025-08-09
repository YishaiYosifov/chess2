using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Grains;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.MatchmakingTests;

public class MatchmakingServiceTests : BaseIntegrationTest
{
    private readonly GameSettings _settings;
    private readonly MatchmakingService _matchmakingService;

    private readonly IPlayerSessionGrain _playerSessionGrainMock =
        Substitute.For<IPlayerSessionGrain>();
    private readonly IGrainFactory _grainsMock = Substitute.For<IGrainFactory>();

    private readonly TimeControlSettings _timeControl = new(BaseSeconds: 600, IncrementSeconds: 5);
    private readonly ConnectionId _connId = "connid";

    public MatchmakingServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var ratingService = Scope.ServiceProvider.GetRequiredService<IRatingService>();
        var timeControlTranslator =
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>();
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;

        _matchmakingService = new(_grainsMock, ratingService, timeControlTranslator, settings);
    }

    [Fact]
    public async Task SeekRatedAsync_creates_a_rated_seek_with_the_correct_values()
    {
        var user = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(user)
            .RuleFor(x => x.TimeControl, TimeControl.Rapid)
            .Generate();

        await DbContext.AddRangeAsync(user, rating);
        await DbContext.SaveChangesAsync(CT);
        _grainsMock.GetGrain<IPlayerSessionGrain>(user.Id).Returns(_playerSessionGrainMock);

        await _matchmakingService.SeekRatedAsync(user, _connId, _timeControl);

        SeekerRating expectedRating = new(rating.Value, _settings.AllowedMatchRatingDifference);
        RatedSeeker expectedSeeker = new(
            user.Id,
            user.UserName!,
            BlockedUserIds: [],
            Rating: expectedRating
        );
        PoolKey expectedPoolKey = new(PoolType.Rated, _timeControl);
        await _playerSessionGrainMock
            .Received(1)
            .CreateSeekAsync(
                _connId,
                ArgEx.FluentAssert<Seeker>(x => x.Should().BeEquivalentTo(expectedSeeker)),
                expectedPoolKey
            );
    }

    [Fact]
    public async Task SeekCasualAsync_creates_a_casual_seek_with_the_correct_values_for_a_guest()
    {
        UserId userId = "test guest user";
        _grainsMock.GetGrain<IPlayerSessionGrain>(userId).Returns(_playerSessionGrainMock);

        await _matchmakingService.SeekCasualAsync(
            userId,
            _connId,
            user: null,
            timeControl: _timeControl
        );

        Seeker expectedSeeker = new(userId, "Guest", BlockedUserIds: []);
        PoolKey expectedPoolKey = new(PoolType.Casual, _timeControl);
        await _playerSessionGrainMock
            .Received(1)
            .CreateSeekAsync(
                _connId,
                ArgEx.FluentAssert<Seeker>(x => x.Should().BeEquivalentTo(expectedSeeker)),
                expectedPoolKey
            );
    }

    [Fact]
    public async Task SeekCasualAsync_creates_a_casual_seek_with_the_correct_values_for_an_authed_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user);
        _grainsMock.GetGrain<IPlayerSessionGrain>(user.Id).Returns(_playerSessionGrainMock);

        await _matchmakingService.SeekCasualAsync(user.Id, _connId, user, _timeControl);

        Seeker expectedSeeker = new(user.Id, user.UserName!, BlockedUserIds: []);
        PoolKey expectedPoolKey = new(PoolType.Casual, _timeControl);
        await _playerSessionGrainMock
            .Received(1)
            .CreateSeekAsync(
                _connId,
                ArgEx.FluentAssert<Seeker>(x => x.Should().BeEquivalentTo(expectedSeeker)),
                expectedPoolKey
            );
    }

    [Fact]
    public async Task CancelSeekAsync_cancels_the_seek_for_the_user_id()
    {
        UserId userId = "test user";
        _grainsMock.GetGrain<IPlayerSessionGrain>(userId).Returns(_playerSessionGrainMock);

        await _matchmakingService.CancelSeekAsync(userId, _connId);

        await _playerSessionGrainMock.Received(1).CancelSeekAsync(_connId);
    }
}
