using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class OpenSeekNotifierTests
{
    private readonly List<string> _userIds = ["user1", "user2", "user3"];

    private readonly IHubContext<OpenSeekHub, IOpenSeekHubClient> _hubContextMock = Substitute.For<
        IHubContext<OpenSeekHub, IOpenSeekHubClient>
    >();
    private readonly IHubClients<IOpenSeekHubClient> _clientsMock = Substitute.For<
        IHubClients<IOpenSeekHubClient>
    >();
    private readonly IOpenSeekHubClient _clientProxyMock = Substitute.For<IOpenSeekHubClient>();

    private readonly OpenSeekNotifier _notifier;

    public OpenSeekNotifierTests()
    {
        _clientsMock
            .Users(Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(_userIds)))
            .Returns(_clientProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyOpenSeekAsync_notifies_all_users_of_open_seeks()
    {
        List<OpenSeek> seeks =
        [
            new OpenSeek(
                SeekKey: new SeekKey(
                    "user id",
                    new PoolKey(PoolType.Rated, new TimeControlSettings(600, 3))
                ),
                UserName: "username",
                TimeControl: TimeControl.Rapid,
                Rating: 123
            ),
            new OpenSeek(
                SeekKey: new SeekKey(
                    "user id",
                    new PoolKey(PoolType.Casual, new TimeControlSettings(6, 35))
                ),
                UserName: "username",
                TimeControl: TimeControl.Blitz,
                Rating: null
            ),
        ];

        await _notifier.NotifyOpenSeekAsync(_userIds, seeks);

        await _clientProxyMock.Received(1).NewOpenSeekAsync(seeks);
    }

    [Fact]
    public async Task NotifyOpenSeekEndedAsync_notifies_all_users_of_ended_seeks()
    {
        SeekKey seekKey = new(
            "test user id",
            new PoolKey(PoolType.Rated, new TimeControlSettings(600, 3))
        );

        await _notifier.NotifyOpenSeekEndedAsync(_userIds, seekKey);

        await _clientProxyMock.Received(1).OpenSeekEndedAsync(seekKey);
    }
}
