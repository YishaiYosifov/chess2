using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using ErrorOr;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.TestKit;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameGrainTests : BaseGrainTest
{
    private const string TestGameToken = "testtoken";

    private readonly PoolKey _pool = new(PoolType.Rated, new(600, 5));
    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameGrainTests()
    {
        Silo.ServiceProvider.AddService(Options.Create(AppSettingsLoader.LoadAppSettings()));
    }

    [Fact]
    public async Task StartGameAsync_initializes_the_game_and_transitions_to_playing_state()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(0);

        await StartGameAsync(grain);

        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(1);
        var context = Silo.GetContextFromGrain(grain);
        Silo.TimerRegistry.Mock.Verify(x =>
            x.RegisterGrainTimer(
                context,
                It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                It.IsAny<It.IsAnyType>(),
                new() { DueTime = TimeSpan.Zero, Period = TimeSpan.FromSeconds(1) }
            )
        );
        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                GameGrain.ClockReactivationReminder,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(5)
            )
        );
    }

    [Fact]
    public async Task ReceiveReminder_does_nothing_when_game_is_not_over()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);
        Silo.TimerRegistry.Mock.Reset();

        await Silo.FireAllReminders();

        var context = Silo.GetContextFromGrain(grain);
        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(1);
        Silo.ReminderRegistry.Mock.Verify(
            x => x.UnregisterReminder(It.IsAny<GrainId>(), It.IsAny<IGrainReminder>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ReceiveReminder_unregisters_itself_when_game_is_over()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);
        Silo.TimerRegistry.Mock.Reset();

        var state = Silo.StorageManager.GetStorage<GameGrainState>(GameGrain.StateName);
        state.State.CurrentGame!.Result = new GameResultDataFaker().Generate();
        await state.WriteStateAsync(CT);

        await Silo.FireAllReminders();

        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(1);
        Silo.ReminderRegistry.Mock.Verify(x =>
            x.UnregisterReminder(
                Silo.GetGrainId(grain),
                It.Is<IGrainReminder>(r => r.ReminderName == GameGrain.ClockReactivationReminder)
            )
        );
    }

    [Fact]
    public async Task OnActivateAsync_restarts_timer_when_game_is_not_over()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);
        Silo.TimerRegistry.Mock.Reset();

        await Silo.DeactivateAsync(grain, cancellationToken: CT);
        await grain.OnActivateAsync(CT);

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
    public async Task OnActivateAsync_doesnt_restart_timer_when_game_is_over()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);
        await StartGameAsync(grain);
        Silo.TimerRegistry.Mock.Reset();

        var state = Silo.StorageManager.GetStorage<GameGrainState>(GameGrain.StateName);
        state.State.CurrentGame!.Result = new GameResultDataFaker().Generate();
        await state.WriteStateAsync(CT);

        await Silo.DeactivateAsync(grain, cancellationToken: CT);
        await grain.OnActivateAsync(CT);

        Silo.TimerRegistry.Mock.Verify(
            x =>
                x.RegisterGrainTimer(
                    It.IsAny<IGrainContext>(),
                    It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<GrainTimerCreationOptions>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task OnActivateAsync_doesnt_restart_timer_when_game_doesnt_exist()
    {
        var grain = await Silo.CreateGrainAsync<GameGrain>(TestGameToken);

        await Silo.DeactivateAsync(grain, cancellationToken: CT);
        await grain.OnActivateAsync(CT);

        Silo.TimerRegistry.Mock.Verify(
            x =>
                x.RegisterGrainTimer(
                    It.IsAny<IGrainContext>(),
                    It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<GrainTimerCreationOptions>()
                ),
            Times.Never
        );
    }

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
                new(from: new AlgebraicPoint("a2"), to: new AlgebraicPoint("c4"))
            )
        );

    [Fact]
    public Task MovePieceAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.MovePieceAsync(
                _whitePlayer.UserId,
                new(from: new AlgebraicPoint("a2"), to: new AlgebraicPoint("c4"))
            )
        );

    [Fact]
    public Task EndGameAsync_rejects_invalid_users() =>
        AssertRejectsForInvalidPlayerAsync(async grain =>
            await grain.RequestGameEndAsync("invalid user")
        );

    [Fact]
    public Task EndGameAsync_rejects_when_not_playing() =>
        AssertRejectsForNotPlayingAsync(async grain =>
            await grain.RequestGameEndAsync(_whitePlayer.UserId)
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
    }

    private Task StartGameAsync(GameGrain grain) =>
        grain.StartGameAsync(
            whitePlayer: _whitePlayer,
            blackPlayer: _blackPlayer,
            pool: _pool,
            GameSource.Unknown,
            CT
        );
}
