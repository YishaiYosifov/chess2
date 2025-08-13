using Chess2.Api.Lobby.Services;
using Chess2.Api.Lobby.SignalR;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class LobbyNotifierTests
{
    private const string ConnectionId = "testconn";

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
        _clientsMock.Client(ConnectionId).Returns(_clientProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyGameFoundAsync_notifies_correct_method()
    {
        var gameToken = "game456";

        await _notifier.NotifyGameFoundAsync(ConnectionId, gameToken);

        await _clientProxyMock.Received(1).MatchFoundAsync(gameToken);
    }

    [Fact]
    public async Task NotifyMatchFailedAsync_notifies_correct_method()
    {
        await _notifier.NotifyMatchFailedAsync(ConnectionId);

        await _clientProxyMock.Received(1).MatchFailedAsync();
    }
}
