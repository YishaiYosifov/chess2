using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.SignalR;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingNotifierTests
{
    private const string UserId = "testuser";

    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _hubContextMock =
        Substitute.For<IHubContext<MatchmakingHub, IMatchmakingClient>>();
    private readonly IHubClients<IMatchmakingClient> _clientsMock = Substitute.For<
        IHubClients<IMatchmakingClient>
    >();
    private readonly IMatchmakingClient _clientProxyMock = Substitute.For<IMatchmakingClient>();

    private readonly MatchmakingNotifier _notifier;

    public MatchmakingNotifierTests()
    {
        _clientsMock.User(UserId).Returns(_clientProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyGameFoundAsync_notifies_correct_method()
    {
        var gameToken = "game456";

        await _notifier.NotifyGameFoundAsync(UserId, gameToken);

        await _clientProxyMock.Received(1).MatchFoundAsync(gameToken);
    }

    [Fact]
    public async Task NotifyMatchFailedAsync_notifies_correct_method()
    {
        await _notifier.NotifyMatchFailedAsync(UserId);

        await _clientProxyMock.Received(1).MatchFailedAsync();
    }
}
