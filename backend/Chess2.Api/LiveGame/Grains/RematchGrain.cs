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
    [Alias("RequestRematch")]
    Task<ErrorOr<Success>> RequestRematch(UserId requestedBy);
}

public record RematchRequest(PlayerRoster Players, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Grains.RematchGrainState")]
public class RematchGrainState
{
    [Id(0)]
    public bool WhiteRequested { get; set; }

    [Id(1)]
    public bool BlackRequested { get; set; }

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

    public async Task<ErrorOr<Success>> RequestRematch(UserId requestedBy)
    {
        var request = _state.State.Request;
        if (request is null)
            return GameErrors.GameNotFound;

        if (!request.Players.TryGetPlayerById(requestedBy, out var player))
            return GameErrors.PlayerInvalid;

        player.Color.Match(
            whenWhite: () => _state.State.WhiteRequested = true,
            whenBlack: () => _state.State.BlackRequested = true
        );

        if (_state.State.WhiteRequested && _state.State.BlackRequested)
        {
            await AcceptRematchAsync(request);
            return Result.Success;
        }

        await this.RegisterOrUpdateReminder(
            ExpirationReminderName,
            _settings.RematchLifetime,
            _settings.RematchLifetime
        );
        await _state.WriteStateAsync();

        var opponent = request.Players.GetPlayerByColor(player.Color.Invert());
        await _rematchNotifier.NotifyRematchRequestedAsync(opponent.UserId);

        return Result.Success;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != ExpirationReminderName)
            return;

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
}
