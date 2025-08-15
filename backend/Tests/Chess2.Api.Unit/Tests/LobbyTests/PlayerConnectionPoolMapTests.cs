using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class PlayerConnectionPoolMapTests
{
    private readonly PlayerConnectionPoolMap _connMap = new();

    private static PoolKey CreatePool(PoolType type, int baseSeconds, int incrementSeconds) =>
        new(type, new TimeControlSettings(baseSeconds, incrementSeconds));

    [Fact]
    public void AddConnectionToPool_adds_connection_and_pool()
    {
        ConnectionId conn = new("conn1");
        var pool = CreatePool(PoolType.Rated, 300, 5);

        _connMap.AddConnectionToPool(conn, pool);

        _connMap.SeekCount.Should().Be(1);
    }

    [Fact]
    public void AddConnectionToPool_allows_multiple_pools_for_same_connection()
    {
        ConnectionId conn = new("conn1");

        _connMap.AddConnectionToPool(conn, CreatePool(PoolType.Rated, 300, 5));
        _connMap.AddConnectionToPool(conn, CreatePool(PoolType.Casual, 600, 0));

        _connMap.SeekCount.Should().Be(2);
    }

    [Fact]
    public void AddConnectionToPool_allows_multiple_connections_for_same_pool()
    {
        var pool = CreatePool(PoolType.Rated, 300, 5);

        _connMap.AddConnectionToPool(new ConnectionId("c1"), pool);
        _connMap.AddConnectionToPool(new ConnectionId("c2"), pool);

        _connMap.SeekCount.Should().Be(2);
    }

    [Fact]
    public void RemoveConnection_removes_connection_but_returns_empty_when_pool_still_used()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        var pool = CreatePool(PoolType.Rated, 300, 5);

        _connMap.AddConnectionToPool(conn1, pool);
        _connMap.AddConnectionToPool(conn2, pool);

        var removedPools = _connMap.RemoveConnection(conn1);

        removedPools.Should().BeEmpty();
        _connMap.SeekCount.Should().Be(1);
    }

    [Fact]
    public void RemoveConnection_returns_pool_when_last_connection_removed()
    {
        ConnectionId conn = new("c1");
        var pool = CreatePool(PoolType.Rated, 300, 5);

        _connMap.AddConnectionToPool(conn, pool);
        var removedPools = _connMap.RemoveConnection(conn);

        removedPools.Should().ContainSingle().Which.Should().Be(pool);
        _connMap.SeekCount.Should().Be(0);
    }

    [Fact]
    public void RemoveConnection_returns_empty_when_connection_not_found()
    {
        var removedPools = _connMap.RemoveConnection(new ConnectionId("nonexistent"));

        removedPools.Should().BeEmpty();
        _connMap.SeekCount.Should().Be(0);
    }

    [Fact]
    public void RemovePool_removes_pool_and_all_connections()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        var pool = CreatePool(PoolType.Casual, 600, 0);

        _connMap.AddConnectionToPool(conn1, pool);
        _connMap.AddConnectionToPool(conn2, pool);

        var removedConnections = _connMap.RemovePool(pool);

        removedConnections.Should().BeEquivalentTo([conn1, conn2]);
        _connMap.SeekCount.Should().Be(0);
    }

    [Fact]
    public void RemovePool_returns_empty_when_pool_not_found()
    {
        var removedConnections = _connMap.RemovePool(CreatePool(PoolType.Rated, 300, 5));

        removedConnections.Should().BeEmpty();
    }

    [Fact]
    public void SeekCount_reflects_multiple_connections_and_pools()
    {
        _connMap.AddConnectionToPool(new ConnectionId("c1"), CreatePool(PoolType.Rated, 300, 5));
        _connMap.AddConnectionToPool(new ConnectionId("c1"), CreatePool(PoolType.Casual, 600, 0));
        _connMap.AddConnectionToPool(new ConnectionId("c2"), CreatePool(PoolType.Rated, 300, 5));

        _connMap.SeekCount.Should().Be(3);
    }
}
