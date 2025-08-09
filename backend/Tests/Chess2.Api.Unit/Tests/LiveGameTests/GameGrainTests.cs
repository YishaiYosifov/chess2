using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.TestInfrastructure.Fakes;
using ErrorOr;
using FluentAssertions;
using Moq;
using Orleans.TestKit;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameGrainTests : BaseGrainTest
{
    private const string TestGameToken = "testtoken";

    private readonly TimeControlSettings _timeControl = new(600, 5);
    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    [Fact]
    public async Task StartGame_initializes_the_game_and_transitions_to_playing_state()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(1);

        await StartGameAsync(grain);

        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(2);
        var context = Silo.GetContextFromGrain(grain);
        Silo.TimerRegistry.Mock.Verify(x =>
            x.RegisterGrainTimer(
                context,
                It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                It.IsAny<It.IsAnyType>(),
                new() { DueTime = TimeSpan.Zero, Period = TimeSpan.FromSeconds(1) }
            )
        );
    }

    [Fact]
    public async Task KeepGameAliveAsync_delays_deactivation_when_playing()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);

        await Silo.FireTimerAsync(0);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(
            x => x.DelayDeactivation(context, TimeSpan.FromMinutes(2)),
            Times.Once
        );
    }

    [Fact]
    public Task GetStateAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain =>
            await grain.GetStateAsync("invalid user")
        );

    [Fact]
    public Task GetStateAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.GetStateAsync(_whitePlayer.UserId)
        );

    [Fact]
    public Task RequestDrawAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain =>
            await grain.RequestDrawAsync("invalid user")
        );

    [Fact]
    public Task RequestDrawAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.RequestDrawAsync(_whitePlayer.UserId)
        );

    [Fact]
    public Task DeclineDrawAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain =>
            await grain.DeclineDrawAsync("invalid user")
        );

    [Fact]
    public Task DeclineDrawAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.DeclineDrawAsync(_whitePlayer.UserId)
        );

    [Fact]
    public Task MovePieceAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain =>
            await grain.MovePieceAsync(
                _blackPlayer.UserId,
                new(From: new AlgebraicPoint("a2"), To: new AlgebraicPoint("c4"))
            )
        );

    [Fact]
    public Task MovePieceAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.MovePieceAsync(
                _whitePlayer.UserId,
                new(From: new AlgebraicPoint("a2"), To: new AlgebraicPoint("c4"))
            )
        );

    [Fact]
    public Task EndGameAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain => await grain.EndGameAsync("invalid user"));

    [Fact]
    public Task EndGameAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.EndGameAsync(_whitePlayer.UserId)
        );

    private async Task AssertRejectsForInvalidPlayerAsync<T>(
        Func<GameGrain, Task<ErrorOr<T>>> callback
    )
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);

        var result = await callback(grain);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    private async Task AssertRejectsForNotPlayingAsync<T>(
        Func<GameGrain, Task<ErrorOr<T>>> callback
    )
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);

        var result = await callback(grain);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.GameNotFound);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(x => x.DeactivateOnIdle(context), Times.Once);
    }

    private Task StartGameAsync(GameGrain grain) =>
        grain.StartGameAsync(
            whitePlayer: _whitePlayer,
            blackPlayer: _blackPlayer,
            timeControl: _timeControl,
            isRated: true
        );
}
