using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class LobbyNotifierTests
{
    private readonly List<ConnectionId> _connectionIds = ["conn1", "conn2", "conn3"];

    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hubContextMock = Substitute.For<
        IHubContext<LobbyHub, ILobbyHubClient>
    >();
    private readonly IHubClients<ILobbyHubClient> _clientsMock = Substitute.For<
        IHubClients<ILobbyHubClient>
    >();
    private readonly ILobbyHubClient _clientProxyMock = Substitute.For<ILobbyHubClient>();

    private readonly LobbyNotifier _notifier;

    public LobbyNotifierTests()
    {
        _clientsMock
            .Clients(
                Arg.Is<IReadOnlyList<string>>(x =>
                    x.SequenceEqual(_connectionIds.Select(x => x.Value))
                )
            )
            .Returns(_clientProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyGameFoundAsync_notifies_correct_method()
    {
        var gameToken = "game456";

        await _notifier.NotifyGameFoundAsync(_connectionIds, gameToken);

        await _clientProxyMock.Received(1).MatchFoundAsync(gameToken);
    }

    [Fact]
    public async Task NotifySeekFailedAsync_notifies_correct_method()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(300, 5));

        await _notifier.NotifySeekFailedAsync(_connectionIds, pool);

        await _clientProxyMock.Received(1).SeekFailedAsync(pool);
    }
}
