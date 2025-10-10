using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.LiveGame.SignalR;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class RematchNotifierTests
{
    private readonly UserId _user1Id = "user-1";
    private readonly UserId _user2Id = "user-2";

    private readonly IHubContext<GameHub, IGameHubClient> _hubContextMock = Substitute.For<
        IHubContext<GameHub, IGameHubClient>
    >();
    private readonly IHubClients<IGameHubClient> _clientsMock = Substitute.For<
        IHubClients<IGameHubClient>
    >();

    private readonly IGameHubClient _user1ProxyMock = Substitute.For<IGameHubClient>();
    private readonly IGameHubClient _user2ProxyMock = Substitute.For<IGameHubClient>();

    private readonly RematchNotifier _notifier;

    public RematchNotifierTests()
    {
        _clientsMock.User(_user1Id).Returns(_user1ProxyMock);
        _clientsMock.User(_user2Id).Returns(_user2ProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);

        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyRematchRequestedAsync_notifies_the_correct_user()
    {
        await _notifier.NotifyRematchRequestedAsync(_user2Id);

        await _user2ProxyMock.Received(1).RematchRequestedAsync();
    }

    [Fact]
    public async Task NotifyRematchCancelledAsync_notifies_both_users()
    {
        await _notifier.NotifyRematchCancelledAsync(_user1Id, _user2Id);

        await _user1ProxyMock.Received(1).RematchCancelledAsync();
        await _user2ProxyMock.Received(1).RematchCancelledAsync();
    }

    [Fact]
    public async Task NotifyRematchAccepted_notifies_both_users_with_game_token()
    {
        GameToken gameToken = "game-token-abc";

        await _notifier.NotifyRematchAccepted(gameToken, _user1Id, _user2Id);

        await _user1ProxyMock.Received(1).RematchAccepted(gameToken);
        await _user2ProxyMock.Received(1).RematchAccepted(gameToken);
    }
}
