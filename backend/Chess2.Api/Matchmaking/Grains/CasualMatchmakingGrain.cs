using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.ICasualMatchmakingGrain")]
public interface ICasualMatchmakingGrain : IMatchmakingGrain;

public class CasualMatchmakingGrain(
    ILogger<CasualMatchmakingGrain> logger,
    ILiveGameService liveGameService,
    IOptions<AppSettings> settings,
    ICasualMatchmakingPool pool
)
    : AbstractMatchmakingGrain<ICasualMatchmakingPool>(logger, liveGameService, settings, pool),
        ICasualMatchmakingGrain;
