using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Profile.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameFinalizerTests : BaseUnitTest
{
    private readonly GameFinalizer _gameFinalizer;

    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();
    private readonly IRatingService _ratingServiceMock = Substitute.For<IRatingService>();
    private readonly IGameArchiveService _gameArchiveServiceMock =
        Substitute.For<IGameArchiveService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly IPlayerSessionGrain _whitePlayerSessionGrain =
        Substitute.For<IPlayerSessionGrain>();
    private readonly IPlayerSessionGrain _blackPlayerSessionGrain =
        Substitute.For<IPlayerSessionGrain>();
    private readonly IGrainFactory _grainFactory = Substitute.For<IGrainFactory>();

    private const string GameToken = "test game token 123";
    private readonly TimeControl _timeControl = TimeControl.Rapid;
    private readonly TimeControlSettings _timeControlSettings = new(
        BaseSeconds: 600,
        IncrementSeconds: 5
    );

    public GameFinalizerTests()
    {
        _gameFinalizer = new(
            _userManagerMock,
            _ratingServiceMock,
            _gameArchiveServiceMock,
            new TimeControlTranslator(),
            _grainFactory,
            _unitOfWorkMock
        );
    }

    [Fact]
    public async Task FinalizeGame_creates_archive_and_updates_rating_correctly()
    {
        var (whiteUser, whitePlayer, blackUser, blackPlayer) = CreatePlayers();

        var state = CreateRatedGameState(whitePlayer, blackPlayer);
        RatingChange ratingChange = new(15, -15);
        GameEndStatus endStatus = new(Result: GameResult.WhiteWin, ResultDescription: "desc");

        _ratingServiceMock
            .UpdateRatingForResultAsync(whiteUser, blackUser, endStatus.Result, _timeControl, CT)
            .Returns(ratingChange);
        _grainFactory.GetGrain<IPlayerSessionGrain>(whiteUser.Id).Returns(_whitePlayerSessionGrain);
        _grainFactory.GetGrain<IPlayerSessionGrain>(blackUser.Id).Returns(_blackPlayerSessionGrain);

        await _gameFinalizer.FinalizeGameAsync(GameToken, state, endStatus, CT);

        await _gameArchiveServiceMock
            .Received(1)
            .CreateArchiveAsync(GameToken, state, endStatus, ratingChange, CT);
        await _ratingServiceMock
            .Received(1)
            .UpdateRatingForResultAsync(
                whiteUser,
                blackUser,
                endStatus.Result,
                _timeControl,
                Arg.Any<CancellationToken>()
            );
        await _unitOfWorkMock.Received(1).CompleteAsync(CT);

        await _whitePlayerSessionGrain.Received(1).GameEndedAsync(GameToken);
        await _blackPlayerSessionGrain.Received(1).GameEndedAsync(GameToken);
    }

    [Fact]
    public async Task FinalizeGameAsync_aborted_game_does_not_call_rating_service()
    {
        var (_, whitePlayer, _, blackPlayer) = CreatePlayers();

        var state = CreateRatedGameState(whitePlayer, blackPlayer);

        await _gameFinalizer.FinalizeGameAsync(
            GameToken,
            state,
            new(GameResult.Aborted, "desc"),
            CT
        );

        _ratingServiceMock.Received(0);
    }

    [Fact]
    public async Task FinalizeGameAsync_unrated_game_does_not_call_rating_service()
    {
        var state = new GameStateFaker()
            .RuleFor(x => x.Pool, new PoolKey(PoolType.Casual, _timeControlSettings))
            .Generate();
        var ratingChange = new RatingChange(15, -15);

        await _gameFinalizer.FinalizeGameAsync(
            GameToken,
            state,
            new(GameResult.WhiteWin, "desc"),
            CT
        );

        _ratingServiceMock.Received(0);
    }

    private (
        AuthedUser whiteUser,
        GamePlayer whitePlayer,
        AuthedUser blackUser,
        GamePlayer blackPlayer
    ) CreatePlayers()
    {
        var user1 = new AuthedUserFaker().Generate();
        var whitePlayer = new GamePlayerFaker(GameColor.White, user1);

        var user2 = new AuthedUserFaker().Generate();
        var blackPlayer = new GamePlayerFaker(GameColor.Black, user2);

        _userManagerMock.FindByIdAsync(user1.Id).Returns(user1);
        _userManagerMock.FindByIdAsync(user2.Id).Returns(user2);

        return (user1, whitePlayer, user2, blackPlayer);
    }

    private GameState CreateRatedGameState(GamePlayer whitePlayer, GamePlayer blackPlayer)
    {
        return new GameStateFaker()
            .RuleFor(x => x.WhitePlayer, whitePlayer)
            .RuleFor(x => x.BlackPlayer, blackPlayer)
            .RuleFor(x => x.Pool, new PoolKey(PoolType.Rated, _timeControlSettings))
            .Generate();
    }
}
