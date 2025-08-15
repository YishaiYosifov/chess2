using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Lobby.Services;

public class PlayerConnectionPoolMap
{
    private readonly Dictionary<ConnectionId, HashSet<PoolKey>> _connectionToPools = [];
    private readonly Dictionary<PoolKey, HashSet<ConnectionId>> _poolToConnections = [];
    public int SeekCount => _connectionToPools.Values.Sum(pools => pools.Count);

    public void AddConnectionToPool(ConnectionId connectionId, PoolKey poolKey)
    {
        if (_connectionToPools.TryGetValue(connectionId, out var pools))
        {
            pools.Add(poolKey);
        }
        else
        {
            _connectionToPools[connectionId] = [poolKey];
        }

        if (_poolToConnections.TryGetValue(poolKey, out var connections))
        {
            connections.Add(connectionId);
        }
        else
        {
            _poolToConnections[poolKey] = [connectionId];
        }
    }

    public IReadOnlyList<PoolKey> RemoveConnection(ConnectionId connectionId)
    {
        if (!_connectionToPools.TryGetValue(connectionId, out var connectionPools))
            return [];

        List<PoolKey> removedPools = [];
        foreach (var pool in connectionPools)
        {
            if (!_poolToConnections.TryGetValue(pool, out var connections))
                continue;

            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _poolToConnections.Remove(pool);
                removedPools.Add(pool);
            }
        }
        _connectionToPools.Remove(connectionId);
        return removedPools;
    }

    public IReadOnlyList<ConnectionId> RemovePool(PoolKey poolKey)
    {
        if (!_poolToConnections.TryGetValue(poolKey, out var connections))
            return [];

        foreach (var connection in connections)
        {
            if (!_connectionToPools.TryGetValue(connection, out var connectionPools))
                continue;

            connectionPools.Remove(poolKey);
            if (connectionPools.Count == 0)
                _connectionToPools.Remove(connection);
        }
        _poolToConnections.Remove(poolKey);

        return [.. connections];
    }
}
