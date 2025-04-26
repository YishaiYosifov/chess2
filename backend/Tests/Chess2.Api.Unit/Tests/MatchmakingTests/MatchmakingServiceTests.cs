using AutoFixture;
using Chess2.Api.Models;
using Chess2.Api.Repositories;
using Chess2.Api.Services.Matchmaking;
using Chess2.Api.SignalR;
using Chess2.Api.TestInfrastructure.Fakes;
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
    private readonly IOptions<AppSettings> _settings;

    private readonly MatchmakingService _matchmakingService;

    public MatchmakingServiceTests()
    {
        _settings = Fixture.Create<IOptions<AppSettings>>();

        _matchmakingService = new MatchmakingService(
            _settings,
            _ratingRepositoryMock,
            _matchmakingRepositoryMock,
            _hubContextMock
        );
    }

    [Fact]
    public async Task Seeking_when_no_match_is_found_creates_a_new_seek_request()
    {
        const int timeControl = 600;
        const int increment = 3;
        var user = new AuthedUserFaker().Generate();
        var rating = new RatingFaker(user).Generate();

        _ratingRepositoryMock.GetTimeControlRatingAsync(user, rating.TimeControl).Returns(rating);
        _matchmakingRepositoryMock
            .SearchExistingSeekAsync(
                rating.Value,
                _settings.Value.Game.MaxMatchRatingDifference,
                timeControl,
                increment
            )
            .Returns((string?)null);

        await _matchmakingService.SeekAsync(user, timeControl, increment);

        await _matchmakingRepositoryMock
            .Received(1)
            .CreateSeekAsync(user.Id.ToString(), rating.Value, timeControl, increment);
    }

    [Fact]
    public async Task Seeking_when_a_match_is_found_notifies_both_players()
    {
        const int timeControl = 600;
        const int increment = 3;
        const string matchedUserId = "2";
        var user = new AuthedUserFaker().Generate();
        var rating = new RatingFaker(user).Generate();
        _ratingRepositoryMock.GetTimeControlRatingAsync(user, rating.TimeControl).Returns(rating);

        _matchmakingRepositoryMock
            .SearchExistingSeekAsync(
                rating.Value,
                _settings.Value.Game.MaxMatchRatingDifference,
                timeControl,
                increment
            )
            .Returns(matchedUserId);

        var userClientMock = Substitute.For<IMatchmakingClient>();
        _hubContextMock
            .Clients.User(Arg.Is<string>(arg => arg == user.Id.ToString() || arg == matchedUserId))
            .Returns(userClientMock);

        await _matchmakingService.SeekAsync(user, 10, 5);

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
