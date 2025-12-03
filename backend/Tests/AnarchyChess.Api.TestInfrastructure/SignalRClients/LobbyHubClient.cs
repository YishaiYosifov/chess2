using System.Threading.Channels;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnarchyChess.Api.TestInfrastructure.SignalRClients;

public class LobbyHubClient : BaseHubClient
{
    public const string Path = "/api/hub/lobby";

    private readonly TaskCompletionSource<string> _matchTsc = new();

    private readonly Channel<List<OngoingGame>> _ongoingGamesChannel = Channel.CreateUnbounded<
        List<OngoingGame>
    >();

    public LobbyHubClient(HubConnection connection)
        : base(connection)
    {
        Connection.On<string>("MatchFoundAsync", gameToken => _matchTsc.TrySetResult(gameToken));
        Connection.On<List<OngoingGame>>(
            "ReceiveOngoingGamesAsync",
            games => _ongoingGamesChannel.Writer.TryWrite(games)
        );
    }

    public Task SeekRatedAsync(TimeControlSettings timeControl, CancellationToken token) =>
        Connection.InvokeAsync("SeekRatedAsync", timeControl, token);

    public Task SeekCasualAsync(TimeControlSettings timeControl, CancellationToken token) =>
        Connection.InvokeAsync("SeekCasualAsync", timeControl, token);

    public Task CancelSeekAsync(PoolKey pool, CancellationToken token) =>
        Connection.InvokeAsync("CancelSeekAsync", pool, token);

    public Task MatchWithOpenSeekAsync(UserId matchWith, PoolKey pool, CancellationToken token) =>
        Connection.InvokeAsync("MatchWithOpenSeekAsync", matchWith, pool, token);

    public async Task<List<OngoingGame>> GetNextOngoingGamesBatchAsync(CancellationToken token)
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var batch = await _ongoingGamesChannel.Reader.ReadAsync(cts.Token);
        return batch;
    }

    public Task<string> WaitForGameAsync(CancellationToken token) =>
        _matchTsc.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
}
