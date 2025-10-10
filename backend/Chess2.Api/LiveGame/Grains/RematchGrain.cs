using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Grains;

[Alias("Chess2.Api.LiveGame.Grains.IRematchGrain")]
public interface IRematchGrain : IGrainWithStringKey
{
    [Alias("RequestAsync")]
    Task<ErrorOr<Created>> RequestAsync(UserId requestedBy, ConnectionId connectionId);

    [Alias("CancelAsync")]
    Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy);

    [Alias("RemoveConnectionAsync")]
    Task<ErrorOr<Deleted>> RemoveConnectionAsync(UserId ofUserId, ConnectionId connectionId);
}

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Grains.RematchRequest")]
public record RematchRequest(PlayerRoster Players, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Grains.RematchGrainState")]
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
    IGameStateProvider gameStateProvider,
    IRematchNotifier rematchNotifier,
    IGameStarter gameStarter
) : Grain, IRematchGrain, IRemindable
{
    public const string ExpirationReminderName = "expirationReminder";
    public const string StateName = "rematchState";

    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IPersistentState<RematchGrainState> _state = state;
    private readonly IGameStateProvider _gameStateProvider = gameStateProvider;
    private readonly IRematchNotifier _rematchNotifier = rematchNotifier;
    private readonly IGameStarter _gameStarter = gameStarter;

    public async Task<ErrorOr<Created>> RequestAsync(UserId requestedBy, ConnectionId connectionId)
    {
        var request = _state.State.Request;
        if (request is null)
            return GameErrors.GameNotFound;

        if (!request.Players.TryGetPlayerById(requestedBy, out var player))
            return GameErrors.PlayerInvalid;

        var playerConnections = player.Color.Match(
            whenWhite: _state.State.WhiteConnections,
            whenBlack: _state.State.BlackConnections
        );
        playerConnections.Add(connectionId);

        if (_state.State.WhiteConnections.Count > 0 && _state.State.BlackConnections.Count > 0)
        {
            await AcceptRematchAsync(request);
            return Result.Created;
        }

        await this.RegisterOrUpdateReminder(
            ExpirationReminderName,
            _settings.RematchLifetime,
            _settings.RematchLifetime
        );
        await _state.WriteStateAsync();

        var opponent = request.Players.GetPlayerByColor(player.Color.Invert());
        await _rematchNotifier.NotifyRematchRequestedAsync(opponent.UserId);

        return Result.Created;
    }

    public async Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy)
    {
        var request = _state.State.Request;
        if (request is null)
            return GameErrors.GameNotFound;

        if (!request.Players.TryGetPlayerById(cancelledBy, out var _))
            return GameErrors.PlayerInvalid;

        await TearDownRematchAsync();
        return Result.Deleted;
    }

    public async Task<ErrorOr<Deleted>> RemoveConnectionAsync(
        UserId ofUserId,
        ConnectionId connectionId
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
            await TearDownRematchAsync();
            return Result.Deleted;
        }

        await _state.WriteStateAsync();
        return Result.Deleted;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != ExpirationReminderName)
            return;
        await TearDownRematchAsync();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var gameToken = this.GetPrimaryKeyString();
            if (_state.State.Request is not null)
                return;

            var gameResult = await _gameStateProvider.GetGameStateAsync(
                gameToken,
                token: cancellationToken
            );
            if (gameResult.IsError)
                return;
            var game = gameResult.Value;

            // the game is not over
            if (game.ResultData is null)
                return;

            PlayerRoster players = new(game.WhitePlayer, game.BlackPlayer);
            _state.State.Request = new(players, Pool: game.Pool);
            await _state.WriteStateAsync(cancellationToken);
        }
        finally
        {
            await base.OnActivateAsync(cancellationToken);
        }
    }

    private async Task AcceptRematchAsync(RematchRequest request)
    {
        var gameToken = await _gameStarter.StartGameAsync(
            request.Players.WhitePlayer.UserId,
            request.Players.BlackPlayer.UserId,
            pool: request.Pool
        );
        await _rematchNotifier.NotifyRematchAccepted(
            gameToken,
            request.Players.WhitePlayer.UserId,
            request.Players.BlackPlayer.UserId
        );
        await _state.ClearStateAsync();
    }

    private async Task TearDownRematchAsync()
    {
        var request = _state.State.Request;
        if (request is not null)
        {
            await _rematchNotifier.NotifyRematchCancelledAsync(
                request.Players.WhitePlayer.UserId,
                request.Players.BlackPlayer.UserId
            );
        }

        await _state.ClearStateAsync();
    }
}
