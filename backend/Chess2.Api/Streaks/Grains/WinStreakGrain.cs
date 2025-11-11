using Chess2.Api.Game.Grains;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Streaks.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Orleans.Streams;

namespace Chess2.Api.Streaks.Grains;

[Alias("Chess2.Api.Streaks.Grains.IStreakGrain")]
public interface IWinStreakGrain : IGrainWithStringKey;

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
public class WinStreakGrain(
    ILogger<WinStreakGrain> logger,
    IWinStreakService winStreakService,
    UserManager<AuthedUser> userManager
) : Grain, IWinStreakGrain, IAsyncObserver<GameEndedEvent>
{
    private readonly ILogger<WinStreakGrain> _logger = logger;
    private readonly IWinStreakService _winStreakService = winStreakService;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var stream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await stream.SubscribeAsync(this);

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        UserId userId = this.GetPrimaryKeyString();
        if (userId.IsGuest)
            return;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return;

        var gameGrain = GrainFactory.GetGrain<IGameGrain>(@event.GameToken);
        var gameStateResult = await gameGrain.GetStateAsync();
        if (gameStateResult.IsError)
        {
            _logger.LogWarning(
                "Failed to get game state of {GameToken}: {Errors}",
                @event.GameToken,
                gameStateResult.Errors
            );
            return;
        }
        var gameState = gameStateResult.Value;

        if (gameState.Pool.PoolType is not PoolType.Rated)
            return;

        if (gameState.GameSource is not GameSource.Matchmaking)
            return;

        if (@event.EndStatus.Result is not (GameResult.WhiteWin or GameResult.BlackWin))
            return;

        var playerColor =
            gameState.WhitePlayer.UserId == user.Id
                ? gameState.WhitePlayer.Color
                : gameState.BlackPlayer.Color;

        bool isWin =
            @event.EndStatus.Result
            == playerColor.Match(whenWhite: GameResult.WhiteWin, whenBlack: GameResult.BlackWin);
        if (isWin)
            await _winStreakService.IncrementStreakAsync(user, @event.GameToken);
        else
            await _winStreakService.EndStreakAsync(user.Id);
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in streak grain game stream");
        return Task.CompletedTask;
    }
}
