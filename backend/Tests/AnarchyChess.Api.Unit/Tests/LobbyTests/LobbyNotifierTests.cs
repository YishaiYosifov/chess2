using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Lobby.SignalR;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AwesomeAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LobbyTests;

public class LobbyNotifierTests
{
    private readonly List<ConnectionId> _connectionIds = ["conn1", "conn2", "conn3"];
    private readonly UserId _userId = "user123";
    private readonly ConnectionId _connectionId = "conn123";

    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hubContextMock = Substitute.For<
        IHubContext<LobbyHub, ILobbyHubClient>
    >();

    private readonly IHubClients<ILobbyHubClient> _clientsMock = Substitute.For<
        IHubClients<ILobbyHubClient>
    >();

    private readonly ILobbyHubClient _multiClientProxyMock = Substitute.For<ILobbyHubClient>();
    private readonly ILobbyHubClient _userProxyMock = Substitute.For<ILobbyHubClient>();
    private readonly ILobbyHubClient _connectionProxyMock = Substitute.For<ILobbyHubClient>();

    private readonly LobbyNotifier _notifier;

    public LobbyNotifierTests()
    {
        _clientsMock
            .Clients(
                Arg.Is<IReadOnlyList<string>>(ids =>
                    ids.SequenceEqual(_connectionIds.Select(c => c.Value))
                )
            )
            .Returns(_multiClientProxyMock);

        _clientsMock.User(_userId).Returns(_userProxyMock);

        _clientsMock.Client(_connectionId).Returns(_connectionProxyMock);

        _hubContextMock.Clients.Returns(_clientsMock);

        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyGameFoundAsync_notifies_all_expected_methods()
    {
        var game = new OngoingGameFaker().Generate();

        await _notifier.NotifyGameFoundAsync(_userId, _connectionIds, game);

        await _multiClientProxyMock.Received(1).MatchFoundAsync(game.GameToken);

        await _userProxyMock
            .Received(1)
            .ReceiveOngoingGamesAsync(
                ArgEx.FluentAssert<IEnumerable<OngoingGame>>(x => x.Should().BeEquivalentTo([game]))
            );
    }

    [Fact]
    public async Task NotifySeekFailedAsync_notifies_correct_method()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(300, 5));

        await _notifier.NotifySeekFailedAsync(_connectionIds, pool);

        await _multiClientProxyMock.Received(1).SeekFailedAsync(pool);
    }

    [Fact]
    public async Task NotifyOngoingGamesAsync_notifies_correct_connection()
    {
        var games = new OngoingGameFaker().Generate(3);

        await _notifier.NotifyOngoingGamesAsync(_connectionId, games);

        await _connectionProxyMock
            .Received(1)
            .ReceiveOngoingGamesAsync(
                ArgEx.FluentAssert<IEnumerable<OngoingGame>>(x => x.Should().BeEquivalentTo(games))
            );
    }

    [Fact]
    public async Task NotifyOngoingGameEndedAsync_notifies_user_correctly()
    {
        var token = "game999";

        await _notifier.NotifyOngoingGameEndedAsync(_userId, token);

        await _userProxyMock.Received(1).OngoingGameEndedAsync(token);
    }
}
