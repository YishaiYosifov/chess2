using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Game.SignalR;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameChatNotifierTests
{
    private readonly string _gameToken = "game-123";
    private readonly string _connId = "conn1";
    private readonly CancellationToken CT = new();

    private readonly IHubContext<GameHub, IGameHubClient> _hubContextMock = Substitute.For<
        IHubContext<GameHub, IGameHubClient>
    >();
    private readonly IHubClients<IGameHubClient> _clientsMock = Substitute.For<
        IHubClients<IGameHubClient>
    >();

    private readonly IGameHubClient _clientPlayingGroupProxyMock = Substitute.For<IGameHubClient>();
    private readonly IGameHubClient _clientSpectatorsGroupProxyMock =
        Substitute.For<IGameHubClient>();
    private readonly IGameHubClient _clientConnProxyMock = Substitute.For<IGameHubClient>();

    private readonly GameChatNotifier _notifier;

    public GameChatNotifierTests()
    {
        _clientsMock.Group($"{_gameToken}:chat:playing").Returns(_clientPlayingGroupProxyMock);
        _clientsMock
            .Group($"{_gameToken}:chat:spectators")
            .Returns(_clientSpectatorsGroupProxyMock);
        _clientsMock.Client(_connId).Returns(_clientConnProxyMock);

        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task JoinChatAsync_adds_connection_to_playing_group()
    {
        await _notifier.JoinChatAsync(_gameToken, _connId, isPlaying: true, token: CT);

        await _hubContextMock
            .Groups.Received(1)
            .AddToGroupAsync(_connId, $"{_gameToken}:chat:playing", CT);
    }

    [Fact]
    public async Task JoinChatAsync_adds_connection_to_spectators_group()
    {
        await _notifier.JoinChatAsync(_gameToken, _connId, isPlaying: false, token: CT);

        await _hubContextMock
            .Groups.Received(1)
            .AddToGroupAsync(_connId, $"{_gameToken}:chat:spectators", CT);
    }

    [Fact]
    public async Task LeaveChatAsync_removes_connection_from_playing_group()
    {
        await _notifier.LeaveChatAsync(_gameToken, _connId, isPlaying: true, token: CT);

        await _hubContextMock
            .Groups.Received(1)
            .RemoveFromGroupAsync(_connId, $"{_gameToken}:chat:playing", CT);
    }

    [Fact]
    public async Task LeaveChatAsync_removes_connection_from_spectators_group()
    {
        await _notifier.LeaveChatAsync(_gameToken, _connId, isPlaying: false, token: CT);

        await _hubContextMock
            .Groups.Received(1)
            .RemoveFromGroupAsync(_connId, $"{_gameToken}:chat:spectators", CT);
    }

    [Fact]
    public async Task SendMessageAsync_sends_message_to_playing_group_and_acknowledges_sender()
    {
        string userName = "player1";
        string message = "player message";
        TimeSpan cooldown = TimeSpan.FromSeconds(5);

        await _notifier.SendMessageAsync(
            _gameToken,
            userName,
            _connId,
            cooldown,
            message,
            isPlaying: true
        );

        _clientSpectatorsGroupProxyMock.DidNotReceiveWithAnyArgs();
        await _clientPlayingGroupProxyMock.Received(1).ChatMessageAsync(userName, message);
        await _clientConnProxyMock
            .Received(1)
            .ChatMessageDeliveredAsync(cooldown.TotalMilliseconds);
    }

    [Fact]
    public async Task SendMessageAsync_sends_message_to_spectators_group_and_acknowledges_sender()
    {
        string userName = "spectator";
        string message = "spectator message";
        TimeSpan cooldown = TimeSpan.FromSeconds(3);

        await _notifier.SendMessageAsync(
            _gameToken,
            userName,
            _connId,
            cooldown,
            message,
            isPlaying: false
        );

        _clientPlayingGroupProxyMock.DidNotReceiveWithAnyArgs();
        await _clientSpectatorsGroupProxyMock.Received(1).ChatMessageAsync(userName, message);
        await _clientConnProxyMock
            .Received(1)
            .ChatMessageDeliveredAsync(cooldown.TotalMilliseconds);
    }
}
