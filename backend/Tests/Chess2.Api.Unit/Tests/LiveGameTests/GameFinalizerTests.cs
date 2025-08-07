using Akka.TestKit;
using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.PlayerSession.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameFinalizerTests : BaseActorTest
{
    private readonly GameFinalizer _gameFinalizer;

    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();
    private readonly IRatingService _ratingServiceMock = Substitute.For<IRatingService>();
    private readonly IGameArchiveService _gameArchiveServiceMock =
        Substitute.For<IGameArchiveService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ITimeControlTranslator _timeControlTranslatorMock =
        Substitute.For<ITimeControlTranslator>();

    private readonly TestProbe _playerSessionProbe;
    private const string GameToken = "test game token 123";

    public GameFinalizerTests()
    {
        _playerSessionProbe = CreateTestProbe();
        _gameFinalizer = new(
            _userManagerMock,
            _ratingServiceMock,
            _gameArchiveServiceMock,
            _timeControlTranslatorMock,
            _unitOfWorkMock
        );
    }

    [Fact(Skip = "grain")]
    public async Task FinalizeGame_creates_archive_and_updates_rating_correctly()
    {
        var (whiteUser, whitePlayer, blackUser, blackPlayer) = CreatePlayers();

        var state = CreateRatedGameState(whitePlayer, blackPlayer);
        var timeControl = TimeControl.Rapid;
        RatingChange ratingChange = new(15, -15);
        GameEndStatus endStatus = new(Result: GameResult.WhiteWin, ResultDescription: "desc");
        _timeControlTranslatorMock.FromSeconds(state.TimeControl.BaseSeconds).Returns(timeControl);
        _ratingServiceMock
            .UpdateRatingForResultAsync(whiteUser, blackUser, endStatus.Result, timeControl, CT)
            .Returns(ratingChange);

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
                timeControl,
                Arg.Any<CancellationToken>()
            );
        await _unitOfWorkMock.Received(1).CompleteAsync(CT);
        await _playerSessionProbe.FishForMessageAsync<PlayerSessionCommands.GameEnded>(
            msg => msg.UserId == whiteUser.Id,
            cancellationToken: CT
        );
        await _playerSessionProbe.FishForMessageAsync<PlayerSessionCommands.GameEnded>(
            msg => msg.UserId == blackUser.Id,
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task FinalizeGameAsync_aborted_game_does_not_call_rating_service()
    {
        var (_, whitePlayer, _, blackPlayer) = CreatePlayers();

        var state = CreateRatedGameState(whitePlayer, blackPlayer);
        var timeControl = TimeControl.Rapid;

        _timeControlTranslatorMock.FromSeconds(state.TimeControl.BaseSeconds).Returns(timeControl);

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
        var state = new GameStateFaker().RuleFor(x => x.IsRated, false).Generate();
        var ratingChange = new RatingChange(15, -15);
        var timeControl = TimeControl.Rapid;

        _timeControlTranslatorMock.FromSeconds(state.TimeControl.BaseSeconds).Returns(timeControl);

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

    private static GameState CreateRatedGameState(GamePlayer whitePlayer, GamePlayer blackPlayer)
    {
        return new GameStateFaker()
            .RuleFor(x => x.WhitePlayer, whitePlayer)
            .RuleFor(x => x.BlackPlayer, blackPlayer)
            .RuleFor(x => x.IsRated, true)
            .Generate();
    }
}
