﻿using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Game.Grains;

[Alias("Chess2.Api.Game.Grains.IRematchGrain")]
public interface IRematchGrain : IGrainWithStringKey
{
    [Alias("RequestAsync")]
    Task<ErrorOr<Created>> RequestAsync(
        UserId requestedBy,
        ConnectionId connectionId,
        CancellationToken token = default
    );

    [Alias("CancelAsync")]
    Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy, CancellationToken token = default);

    [Alias("RemoveConnectionAsync")]
    Task<ErrorOr<Deleted>> RemoveConnectionAsync(
        UserId ofUserId,
        ConnectionId connectionId,
        CancellationToken token = default
    );
}

[GenerateSerializer]
[Alias("Chess2.Api.Game.Grains.RematchRequest")]
public record RematchRequest(PlayerRoster Players, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.Game.Grains.RematchGrainState")]
public class RematchGrainState
{
    [Id(0)]
    public HashSet<ConnectionId> WhiteConnections { get; } = [];

    [Id(1)]
    public HashSet<ConnectionId> BlackConnections { get; } = [];

    [Id(2)]
    public RematchRequest? Request { get; set; }
}

public class RematchGrain(
    [PersistentState(RematchGrain.StateName, StorageNames.RematchState)]
        IPersistentState<RematchGrainState> state,
    IOptions<AppSettings> settings,
    IRematchNotifier rematchNotifier,
    IGameStarter gameStarter
) : Grain, IRematchGrain, IRemindable
{
    public const string ExpirationReminderName = "expirationReminder";
    public const string StateName = "rematchState";

    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IPersistentState<RematchGrainState> _state = state;
    private readonly IRematchNotifier _rematchNotifier = rematchNotifier;
    private readonly IGameStarter _gameStarter = gameStarter;

    public async Task<ErrorOr<Created>> RequestAsync(
        UserId requestedBy,
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        var requestResult = await FetchRematchRequest(token);
        if (requestResult.IsError)
            return requestResult.Errors;
        var request = requestResult.Value;

        if (!request.Players.TryGetPlayerById(requestedBy, out var player))
            return GameErrors.PlayerInvalid;

        var playerConnections = player.Color.Match(
            whenWhite: _state.State.WhiteConnections,
            whenBlack: _state.State.BlackConnections
        );
        playerConnections.Add(connectionId);

        if (_state.State.WhiteConnections.Count > 0 && _state.State.BlackConnections.Count > 0)
        {
            await AcceptRematchAsync(request, token);
            return Result.Created;
        }

        await this.RegisterOrUpdateReminder(
            ExpirationReminderName,
            _settings.RematchLifetime,
            _settings.RematchLifetime
        );
        await _state.WriteStateAsync(token);

        var opponent = request.Players.GetPlayerByColor(player.Color.Invert());
        await _rematchNotifier.NotifyRematchRequestedAsync(opponent.UserId);

        return Result.Created;
    }

    public async Task<ErrorOr<Deleted>> CancelAsync(
        UserId cancelledBy,
        CancellationToken token = default
    )
    {
        var request = _state.State.Request;
        if (request is null)
            return GameErrors.GameNotFound;

        if (!request.Players.TryGetPlayerById(cancelledBy, out var _))
            return GameErrors.PlayerInvalid;

        await TearDownRematchAsync(token);
        return Result.Deleted;
    }

    public async Task<ErrorOr<Deleted>> RemoveConnectionAsync(
        UserId ofUserId,
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        var request = _state.State.Request;
        if (request is null)
            return GameErrors.GameNotFound;

        if (!request.Players.TryGetPlayerById(ofUserId, out var player))
            return GameErrors.PlayerInvalid;

        var playerConnections = player.Color.Match(
            whenWhite: _state.State.WhiteConnections,
            whenBlack: _state.State.BlackConnections
        );
        playerConnections.Remove(connectionId);

        if (playerConnections.Count == 0)
        {
            await TearDownRematchAsync(token);
            return Result.Deleted;
        }

        await _state.WriteStateAsync(token);
        return Result.Deleted;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != ExpirationReminderName)
            return;
        await TearDownRematchAsync();
    }

    private async Task<ErrorOr<RematchRequest>> FetchRematchRequest(
        CancellationToken token = default
    )
    {
        if (_state.State.Request is not null)
            return _state.State.Request;

        var gameToken = this.GetPrimaryKeyString();
        var gameResult = await GrainFactory.GetGrain<IGameGrain>(gameToken).GetStateAsync();
        if (gameResult.IsError)
            return gameResult.Errors;

        var game = gameResult.Value;
        if (game.ResultData is null)
            return GameErrors.GameNotOver;

        PlayerRoster players = new(game.WhitePlayer, game.BlackPlayer);
        RematchRequest request = new(players, Pool: game.Pool);

        _state.State.Request = request;
        await _state.WriteStateAsync(token);

        return request;
    }

    private async Task AcceptRematchAsync(RematchRequest request, CancellationToken token = default)
    {
        var gameToken = await _gameStarter.StartGameAsync(
            request.Players.WhitePlayer.UserId,
            request.Players.BlackPlayer.UserId,
            pool: request.Pool,
            token: token
        );
        await _rematchNotifier.NotifyRematchAccepted(
            gameToken,
            request.Players.WhitePlayer.UserId,
            request.Players.BlackPlayer.UserId
        );
        await _state.ClearStateAsync(token);
    }

    private async Task TearDownRematchAsync(CancellationToken token = default)
    {
        var request = _state.State.Request;
        if (request is not null)
        {
            await _rematchNotifier.NotifyRematchCancelledAsync(
                request.Players.WhitePlayer.UserId,
                request.Players.BlackPlayer.UserId
            );
        }

        await _state.ClearStateAsync(token);
    }
}
