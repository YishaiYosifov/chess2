using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Services;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingNotifierTests
{
    private const string ConnectionId = "testconn";

    private readonly IHubContext<LobbyHub, IMatchmakingHubClient> _hubContextMock = Substitute.For<
        IHubContext<LobbyHub, IMatchmakingHubClient>
    >();
    private readonly IHubClients<IMatchmakingHubClient> _clientsMock = Substitute.For<
        IHubClients<IMatchmakingHubClient>
    >();
    private readonly IMatchmakingHubClient _clientProxyMock =
        Substitute.For<IMatchmakingHubClient>();

    private readonly MatchmakingNotifier _notifier;

    public MatchmakingNotifierTests()
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
