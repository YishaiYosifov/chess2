using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.Api.UserRating.Models;
using AnarchyChess.Api.UserRating.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameFinalizerTests : BaseUnitTest
{
    private readonly GameFinalizer _gameFinalizer;

    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();
    private readonly IRatingService _ratingServiceMock = Substitute.For<IRatingService>();
    private readonly IGameArchiveService _gameArchiveServiceMock =
        Substitute.For<IGameArchiveService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly GameToken _gameToken = "test game token 123";
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

        var result = await _gameFinalizer.FinalizeGameAsync(_gameToken, state, endStatus, CT);

        result
            .Should()
            .Be(
                new GameResultData(
                    endStatus.Result,
                    endStatus.ResultDescription,
                    ratingChange.WhiteChange,
                    ratingChange.BlackChange
                )
            );
        await _gameArchiveServiceMock
            .Received(1)
            .CreateArchiveAsync(_gameToken, state, endStatus, ratingChange, CT);
        await _ratingServiceMock
            .Received(1)
            .UpdateRatingForResultAsync(whiteUser, blackUser, endStatus.Result, _timeControl, CT);
        await _unitOfWorkMock.Received(1).CompleteAsync(CT);
    }

    [Fact]
    public async Task FinalizeGameAsync_aborted_game_does_not_call_rating_service()
    {
        var (_, whitePlayer, _, blackPlayer) = CreatePlayers();

        var state = CreateRatedGameState(whitePlayer, blackPlayer);

        await _gameFinalizer.FinalizeGameAsync(
            _gameToken,
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
            _gameToken,
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
