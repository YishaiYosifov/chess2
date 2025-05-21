using AutoFixture;
using Chess2.Api.Matchmaking.Repositories;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingServiceTests : BaseUnitTest
{
    private readonly IRatingRepository _ratingRepositoryMock = Substitute.For<IRatingRepository>();
    private readonly IMatchmakingRepository _matchmakingRepositoryMock =
        Substitute.For<IMatchmakingRepository>();
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _hubContextMock =
        Substitute.For<IHubContext<MatchmakingHub, IMatchmakingClient>>();
    private readonly ITimeControlTranslator _timeControlTranslatorMock =
        Substitute.For<ITimeControlTranslator>();

    private readonly IOptions<AppSettings> _settings;
    private readonly AuthedUser _testUser;
    private readonly Rating _testRating;

    private readonly MatchmakingService _matchmakingService;

    public MatchmakingServiceTests()
    {
        _settings = Fixture.Create<IOptions<AppSettings>>();
        _matchmakingService = new MatchmakingService(
            _settings,
            _ratingRepositoryMock,
            _matchmakingRepositoryMock,
            _hubContextMock,
            _timeControlTranslatorMock
        );

        _testUser = new AuthedUserFaker().Generate();
        _testRating = new RatingFaker(_testUser).Generate();
        _ratingRepositoryMock
            .GetTimeControlRatingAsync(_testUser, _testRating.TimeControl)
            .Returns(_testRating);
    }

    [Fact]
    public async Task Seeking_when_no_match_is_found_creates_a_new_seek_request()
    {
        const int timeControl = 600;
        const int increment = 3;
        _timeControlTranslatorMock.FromSeconds(timeControl).Returns(_testRating.TimeControl);

        _matchmakingRepositoryMock
            .SearchExistingSeekAsync(
                _testRating.Value,
                _settings.Value.Game.MaxMatchRatingDifference,
                timeControl,
                increment
            )
            .Returns((string?)null);

        await _matchmakingService.SeekAsync(_testUser, timeControl, increment);

        await _matchmakingRepositoryMock
            .Received(1)
            .CreateSeekAsync(_testUser.Id, _testRating.Value, timeControl, increment);
    }

    [Fact]
    public async Task Seeking_when_a_match_is_found_notifies_both_players()
    {
        const int timeControl = 600;
        const int increment = 3;
        const string matchedUserId = "2";
        _timeControlTranslatorMock.FromSeconds(timeControl).Returns(_testRating.TimeControl);

        _matchmakingRepositoryMock
            .SearchExistingSeekAsync(
                _testRating.Value,
                _settings.Value.Game.MaxMatchRatingDifference,
                timeControl,
                increment
            )
            .Returns(matchedUserId);

        var userClientMock = Substitute.For<IMatchmakingClient>();
        _hubContextMock
            .Clients.User(Arg.Is<string>(arg => arg == _testUser.Id || arg == matchedUserId))
            .Returns(userClientMock);

        await _matchmakingService.SeekAsync(_testUser, timeControl, increment);

        await userClientMock.Received(2).MatchFoundAsync("test");
        await _matchmakingRepositoryMock
            .DidNotReceive()
            .CreateSeekAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Cancel_seek_calls_the_repository_with_the_correct_id()
    {
        var userId = "1";

        await _matchmakingService.CancelSeekAsync(userId);

        await _matchmakingRepositoryMock.Received(1).CancelSeekAsync(userId);
    }
}
