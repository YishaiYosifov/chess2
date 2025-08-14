using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class OpenSeekNotifierTests
{
    private readonly string _connId = "test conn";
    private readonly List<string> _userIds = ["user1", "user2", "user3"];
    private readonly List<OpenSeek> _testSeeks =
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

    private readonly IHubContext<OpenSeekHub, IOpenSeekHubClient> _hubContextMock = Substitute.For<
        IHubContext<OpenSeekHub, IOpenSeekHubClient>
    >();
    private readonly IHubClients<IOpenSeekHubClient> _clientsMock = Substitute.For<
        IHubClients<IOpenSeekHubClient>
    >();
    private readonly IOpenSeekHubClient _clientUsersProxyMock =
        Substitute.For<IOpenSeekHubClient>();
    private readonly IOpenSeekHubClient _clientConnProxyMock = Substitute.For<IOpenSeekHubClient>();

    private readonly OpenSeekNotifier _notifier;

    public OpenSeekNotifierTests()
    {
        _clientsMock
            .Users(Arg.Is<IReadOnlyList<string>>(x => x.SequenceEqual(_userIds)))
            .Returns(_clientUsersProxyMock);
        _clientsMock.Client(_connId).Returns(_clientConnProxyMock);

        _hubContextMock.Clients.Returns(_clientsMock);
        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyOpenSeekAsync_notifies_all_users_of_open_seeks()
    {
        await _notifier.NotifyOpenSeekAsync(_userIds, _testSeeks);

        await _clientUsersProxyMock.Received(1).NewOpenSeeksAsync(_testSeeks);
    }

    [Fact]
    public async Task NotifyOpenSeekAsync_with_a_connection_id_notifies_the_correct_client()
    {
        await _notifier.NotifyOpenSeekAsync(_connId, _testSeeks);

        await _clientConnProxyMock.Received(1).NewOpenSeeksAsync(_testSeeks);
    }

    [Fact]
    public async Task NotifyOpenSeekEndedAsync_notifies_all_users_of_ended_seeks()
    {
        SeekKey seekKey = new(
            "test user id",
            new PoolKey(PoolType.Rated, new TimeControlSettings(600, 3))
        );

        await _notifier.NotifyOpenSeekEndedAsync(_userIds, seekKey);

        await _clientUsersProxyMock.Received(1).OpenSeekEndedAsync(seekKey);
    }
}
