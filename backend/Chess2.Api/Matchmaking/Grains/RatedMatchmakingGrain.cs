using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IRatedMatchmakingGrain")]
public interface IRatedMatchmakingGrain : IMatchmakingGrain;

public class RatedMatchmakingGrain(
    ILogger<RatedMatchmakingGrain> logger,
    ILiveGameService liveGameService,
    IOptions<AppSettings> settings,
    IRatedMatchmakingPool pool
)
    : AbstractMatchmakinGrain<IRatedMatchmakingPool>(logger, liveGameService, settings, pool),
        IRatedMatchmakingGrain;
