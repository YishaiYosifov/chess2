using System.Threading.Channels;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Matchmaking.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class OpenSeekHubClient : BaseHubClient
{
    public const string Path = "/api/hub/openseek";

    private readonly Channel<IEnumerable<OpenSeek>> _openSeekCreatedChannel =
        Channel.CreateUnbounded<IEnumerable<OpenSeek>>();
    private readonly Channel<SeekKey> _openSeekRemovedChannel = Channel.CreateUnbounded<SeekKey>();

    public OpenSeekHubClient(HubConnection connection)
        : base(connection)
    {
        Connection.On<IEnumerable<OpenSeek>>(
            "NewOpenSeeksAsync",
            openSeeks => _openSeekCreatedChannel.Writer.TryWrite(openSeeks)
        );

        Connection.On<SeekKey>(
            "OpenSeekEndedAsync",
            seekKey => _openSeekRemovedChannel.Writer.TryWrite(seekKey)
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

    public async Task<List<OpenSeek>> GetOpenSeekBatchesAsync(
        int batchCount,
        CancellationToken token
    )
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        List<OpenSeek> result = new(batchCount);
        for (int i = 0; i < batchCount; i++)
        {
            var seek = await _openSeekCreatedChannel.Reader.ReadAsync(linkedCts.Token);
            result.AddRange(seek);
        }

        return result;
    }

    public async Task<List<SeekKey>> GetOpenSeekRemovedAsync(int count, CancellationToken token)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        List<SeekKey> result = new(count);
        for (int i = 0; i < count; i++)
        {
            var seek = await _openSeekRemovedChannel.Reader.ReadAsync(linkedCts.Token);
            result.Add(seek);
        }

        return result;
    }
}
