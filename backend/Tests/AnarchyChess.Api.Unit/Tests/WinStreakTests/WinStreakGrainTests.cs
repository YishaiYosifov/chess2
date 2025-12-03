using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Streaks.Grains;
using AnarchyChess.Api.Streaks.Services;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using ErrorOr;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Unit.Tests.WinStreakTests;

public class WinStreakGrainTests : BaseGrainTest
{
    private readonly GameToken _gameToken = "test game token";

    private readonly IGameGrain _gameGrainMock = Substitute.For<IGameGrain>();
    private readonly IWinStreakService _winStreakService = Substitute.For<IWinStreakService>();
    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();

    private readonly GameState _gameState = new GameStateFaker()
        .RuleFor(x => x.Pool, new PoolKeyFaker(PoolType.Rated).Generate())
        .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin).Generate())
        .RuleFor(x => x.GameSource, GameSource.Matchmaking)
        .Generate();
    private readonly AuthedUser _whiteUser;
    private readonly AuthedUser _blackUser;

    public WinStreakGrainTests()
    {
        Silo.AddProbe(id =>
        {
            if (id.ToString() == _gameToken)
                return _gameGrainMock;
            return Substitute.For<IGameGrain>();
        });

        Silo.ServiceProvider.AddService(_winStreakService);
        Silo.ServiceProvider.AddService(_userManagerMock);

        _whiteUser = new AuthedUserFaker().RuleFor(x => x.Id, _gameState.WhitePlayer.UserId);
        _blackUser = new AuthedUserFaker().RuleFor(x => x.Id, _gameState.BlackPlayer.UserId);
        _userManagerMock.FindByIdAsync(_whiteUser.Id).Returns(_whiteUser);
        _userManagerMock.FindByIdAsync(_blackUser.Id).Returns(_blackUser);
    }

    private TestStream<GameEndedEvent> ProbeGameEndedStream(UserId userId) =>
        Silo.AddStreamProbe<GameEndedEvent>(
            userId,
            streamNamespace: nameof(GameEndedEvent),
            Streaming.StreamProvider
        );

    [Fact]
    public async Task GameEndedEvent_doesnt_change_streak_when_user_is_guest()
    {
        var userId = UserId.Guest();
        var streamProbe = ProbeGameEndedStream(userId);
        await Silo.CreateGrainAsync<WinStreakGrain>(userId);

        await streamProbe.OnNextAsync(
            new(_gameToken, new GameResultDataFaker(GameResult.WhiteWin).Generate())
        );

        _winStreakService.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task GameEndedEvent_doesnt_change_streak_when_game_is_not_rated()
    {
        _gameGrainMock
            .GetStateAsync()
            .Returns(_gameState with { Pool = new PoolKeyFaker(PoolType.Casual).Generate() });

        var streamProbe = ProbeGameEndedStream(_gameState.WhitePlayer.UserId);
        await Silo.CreateGrainAsync<WinStreakGrain>(_gameState.WhitePlayer.UserId);

        await streamProbe.OnNextAsync(
            new(_gameToken, new GameResultDataFaker(GameResult.WhiteWin).Generate())
        );

        _winStreakService.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task GameEndedEvent_doesnt_change_streak_when_game_source_is_not_matchmaking()
    {
        _gameGrainMock
            .GetStateAsync()
            .Returns(_gameState with { GameSource = GameSource.Challenge });

        var streamProbe = ProbeGameEndedStream(_gameState.WhitePlayer.UserId);
        await Silo.CreateGrainAsync<WinStreakGrain>(_gameState.WhitePlayer.UserId);

        await streamProbe.OnNextAsync(
            new(_gameToken, new GameResultDataFaker(GameResult.WhiteWin).Generate())
        );

        _winStreakService.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(GameResult.Draw)]
    [InlineData(GameResult.Aborted)]
    public async Task GameEndedEvent_doesnt_change_streak_when_result_is_not_decisive(
        GameResult result
    )
    {
        _gameGrainMock.GetStateAsync().Returns(_gameState);

        var streamProbe = ProbeGameEndedStream(_gameState.WhitePlayer.UserId);
        await Silo.CreateGrainAsync<WinStreakGrain>(_gameState.WhitePlayer.UserId);

        await streamProbe.OnNextAsync(new(_gameToken, new GameResultDataFaker(result).Generate()));

        _winStreakService.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.WhiteWin)]
    [InlineData(GameColor.Black, GameResult.BlackWin)]
    public async Task GameEndedEvent_increments_streak_on_win(
        GameColor playerColor,
        GameResult result
    )
    {
        _gameGrainMock.GetStateAsync().Returns(_gameState);

        var user = playerColor.Match(whenWhite: _whiteUser, whenBlack: _blackUser);
        var streamProbe = ProbeGameEndedStream(user.Id);
        await Silo.CreateGrainAsync<WinStreakGrain>(user.Id);

        await streamProbe.OnNextAsync(new(_gameToken, new GameResultDataFaker(result).Generate()));

        await _winStreakService
            .Received()
            .IncrementStreakAsync(user, _gameToken, Arg.Any<CancellationToken>());
        await _winStreakService.DidNotReceiveWithAnyArgs().EndStreakAsync(default, default!);
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin)]
    [InlineData(GameColor.Black, GameResult.WhiteWin)]
    public async Task GameEndedEvent_ends_streak_on_loss(GameColor playerColor, GameResult result)
    {
        _gameGrainMock.GetStateAsync().Returns(_gameState);

        var user = playerColor.Match(whenWhite: _whiteUser, whenBlack: _blackUser);
        var streamProbe = ProbeGameEndedStream(user.Id);
        await Silo.CreateGrainAsync<WinStreakGrain>(user.Id);

        await streamProbe.OnNextAsync(new(_gameToken, new GameResultDataFaker(result).Generate()));

        await _winStreakService.Received().EndStreakAsync(user.Id, Arg.Any<CancellationToken>());
        await _winStreakService
            .DidNotReceiveWithAnyArgs()
            .IncrementStreakAsync(default!, default, default!);
    }
}
