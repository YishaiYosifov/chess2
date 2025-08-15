using Chess2.Api.Lobby.Services;
using Chess2.Api.Lobby.SignalR;
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
        _clientsMock.Clients(_connectionIds.Select(x => x.Value)).Returns(_clientProxyMock);
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
    public async Task NotifyMatchFailedAsync_notifies_correct_method()
    {
        await _notifier.NotifyMatchFailedAsync(_connectionIds);

        await _clientProxyMock.Received(1).MatchFailedAsync();
    }
}
