using System.Threading.Channels;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class OpenSeekHubClient : BaseHubClient
{
    public const string Path = "/api/hub/openseek";

    private readonly Channel<List<OpenSeek>> _openSeekCreatedChannel = Channel.CreateUnbounded<
        List<OpenSeek>
    >();
    private readonly Channel<(UserId SeekerId, PoolKey Pool)> _openSeekRemovedChannel =
        Channel.CreateUnbounded<(UserId SeekerId, PoolKey Pool)>();

    public OpenSeekHubClient(HubConnection connection)
        : base(connection)
    {
        Connection.On<List<OpenSeek>>(
            "NewOpenSeeksAsync",
            openSeeks => _openSeekCreatedChannel.Writer.TryWrite(openSeeks)
        );

        Connection.On<UserId, PoolKey>(
            "OpenSeekEndedAsync",
            (userId, poolKey) => _openSeekRemovedChannel.Writer.TryWrite((userId, poolKey))
        );
    }

    public static async Task<OpenSeekHubClient> CreateSubscribedAsync(
        HubConnection connection,
        CancellationToken token
    )
    {
        OpenSeekHubClient client = new(connection);
        await client.SubscribeAsync(token);
        return client;
    }

    public Task SubscribeAsync(CancellationToken token) =>
        Connection.InvokeAsync("SubscribeAsync", token);

    public async Task<List<OpenSeek>> GetNextOpenSeekBatcheAsync(CancellationToken token)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        var batch = await _openSeekCreatedChannel.Reader.ReadAsync(linkedCts.Token);
        return batch;
    }

    public async Task<(UserId SeekerId, PoolKey Pool)> GetNextOpenSeekRemovedAsync(
        CancellationToken token
    )
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        var seek = await _openSeekRemovedChannel.Reader.ReadAsync(linkedCts.Token);
        return seek;
    }
}
