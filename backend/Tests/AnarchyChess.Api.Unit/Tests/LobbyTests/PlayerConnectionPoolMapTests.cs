using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Shared.Models;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.LobbyTests;

public class PlayerConnectionPoolMapTests
{
    private readonly PlayerConnectionPoolMap _connMap = new();

    [Fact]
    public void IsEmpty_returns_whether_conections_or_pool_exist()
    {
        ConnectionId conn = new("c1");
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));

        _connMap.IsEmpty().Should().BeTrue();

        _connMap.AddConnectionToPool(conn, pool);
        _connMap.IsEmpty().Should().BeFalse();

        _connMap.RemoveConnection(conn);
        _connMap.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void AddConnectionToPool_adds_connection_and_pool()
    {
        ConnectionId conn = new("conn1");
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));

        _connMap.AddConnectionToPool(conn, pool);

        _connMap.ActivePools.Should().ContainSingle().Which.Should().Be(pool);
        _connMap.PoolConnections(pool).Should().ContainSingle().Which.Should().Be(conn);
    }

    [Fact]
    public void AddConnectionToPool_allows_multiple_pools_for_same_connection()
    {
        ConnectionId conn = new("conn1");
        PoolKey pool1 = new(PoolType.Rated, new TimeControlSettings(300, 5));
        PoolKey pool2 = new(PoolType.Casual, new TimeControlSettings(600, 0));

        _connMap.AddConnectionToPool(conn, pool1);
        _connMap.AddConnectionToPool(conn, pool2);

        _connMap.ActivePools.Should().BeEquivalentTo([pool1, pool2]);
        _connMap.PoolConnections(pool1).Should().ContainSingle().Which.Should().Be(conn);
        _connMap.PoolConnections(pool2).Should().ContainSingle().Which.Should().Be(conn);
    }

    [Fact]
    public void AddConnectionToPool_allows_multiple_connections_for_same_pool()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");

        _connMap.AddConnectionToPool(conn1, pool);
        _connMap.AddConnectionToPool(conn2, pool);

        _connMap.ActivePools.Should().ContainSingle().Which.Should().Be(pool);
        _connMap.PoolConnections(pool).Should().BeEquivalentTo([conn1, conn2]);
    }

    [Fact]
    public void RemoveConnection_removes_connection_but_returns_empty_when_pool_still_used()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));

        _connMap.AddConnectionToPool(conn1, pool);
        _connMap.AddConnectionToPool(conn2, pool);

        HashSet<PoolKey> removedPools = _connMap.RemoveConnection(conn1);

        removedPools.Should().BeEmpty();
        _connMap.ActivePools.Should().ContainSingle().Which.Should().Be(pool);
        _connMap.PoolConnections(pool).Should().ContainSingle().Which.Should().Be(conn2);
    }

    [Fact]
    public void RemoveConnection_returns_pool_when_last_connection_removed()
    {
        ConnectionId conn = new("c1");
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));

        _connMap.AddConnectionToPool(conn, pool);
        HashSet<PoolKey> removedPools = _connMap.RemoveConnection(conn);

        removedPools.Should().ContainSingle().Which.Should().Be(pool);
        _connMap.ActivePools.Should().BeEmpty();
    }

    [Fact]
    public void RemoveConnection_returns_empty_when_connection_not_found()
    {
        HashSet<PoolKey> removedPools = _connMap.RemoveConnection(new ConnectionId("nonexistent"));

        removedPools.Should().BeEmpty();
        _connMap.ActivePools.Should().BeEmpty();
    }

    [Fact]
    public void RemovePool_removes_pool_and_all_connections()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(600, 0));

        _connMap.AddConnectionToPool(conn1, pool);
        _connMap.AddConnectionToPool(conn2, pool);

        HashSet<ConnectionId> removedConnections = _connMap.RemovePool(pool);

        removedConnections.Should().BeEquivalentTo([conn1, conn2]);
        _connMap.ActivePools.Should().BeEmpty();
    }

    [Fact]
    public void RemovePool_returns_empty_when_pool_not_found()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        HashSet<ConnectionId> removedConnections = _connMap.RemovePool(pool);

        removedConnections.Should().BeEmpty();
    }

    [Fact]
    public void ActivePools_and_PoolConnections_reflect_multiple_connections_and_pools()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        PoolKey poolRated = new(PoolType.Rated, new TimeControlSettings(300, 5));
        PoolKey poolCasual = new(PoolType.Casual, new TimeControlSettings(600, 0));

        _connMap.AddConnectionToPool(conn1, poolRated);
        _connMap.AddConnectionToPool(conn1, poolCasual);
        _connMap.AddConnectionToPool(conn2, poolRated);

        _connMap.ActivePools.Should().BeEquivalentTo([poolRated, poolCasual]);
        _connMap.PoolConnections(poolRated).Should().BeEquivalentTo([conn1, conn2]);
        _connMap.PoolConnections(poolCasual).Should().ContainSingle().Which.Should().Be(conn1);
    }

    [Fact]
    public void RemoveAllPools_clears_all_pools_and_connections()
    {
        ConnectionId conn1 = new("c1");
        ConnectionId conn2 = new("c2");
        PoolKey poolRated = new(PoolType.Rated, new TimeControlSettings(300, 5));
        PoolKey poolCasual = new(PoolType.Casual, new TimeControlSettings(600, 0));

        _connMap.AddConnectionToPool(conn1, poolRated);
        _connMap.AddConnectionToPool(conn1, poolCasual);
        _connMap.AddConnectionToPool(conn2, poolRated);

        _connMap.RemoveAllPools();

        _connMap.ActivePools.Should().BeEmpty();
        _connMap.PoolConnections(poolRated).Should().BeEmpty();
        _connMap.PoolConnections(poolCasual).Should().BeEmpty();
    }
}
