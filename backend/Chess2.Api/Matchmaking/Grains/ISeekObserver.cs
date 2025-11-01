using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Models;
using Orleans.Concurrency;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.ISeekObserver")]
public interface ISeekObserver : IGrainObserver
{
    [OneWay]
    [Alias("SeekMatched")]
    public Task SeekMatchedAsync(
        GameToken gameToken,
        PoolKey pool,
        CancellationToken token = default
    );

    [OneWay]
    [Alias("SeekRemoved")]
    public Task SeekRemovedAsync(PoolKey pool, CancellationToken token = default);

    [Alias("TryReserveMatchAsync")]
    Task<bool> TryReserveSeekAsync(PoolKey pool);

    [Alias("ReleaseReservationAsync")]
    Task ReleaseReservationAsync(PoolKey pool);
}
