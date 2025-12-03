using AnarchyChess.Api.Matchmaking.Models;
using Orleans.Concurrency;

namespace AnarchyChess.Api.Matchmaking.Grains;

[Alias("AnarchyChess.Api.Matchmaking.Grains.ISeekObserver")]
public interface ISeekObserver : IGrainObserver
{
    [OneWay]
    [Alias("SeekMatched")]
    public Task SeekMatchedAsync(OngoingGame game, CancellationToken token = default);

    [OneWay]
    [Alias("SeekRemoved")]
    public Task SeekRemovedAsync(PoolKey pool, CancellationToken token = default);

    [Alias("TryReserveMatchAsync")]
    Task<bool> TryReserveSeekAsync(PoolKey pool);

    [Alias("ReleaseReservationAsync")]
    Task ReleaseReservationAsync(PoolKey pool);
}
