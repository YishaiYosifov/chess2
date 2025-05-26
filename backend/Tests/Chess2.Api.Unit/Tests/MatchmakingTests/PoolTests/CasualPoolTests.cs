using Chess2.Api.Matchmaking.Services.Pools;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

internal class CasualPoolTests : BasePoolTests<CasualMatchmakingPool>
{
    private readonly CasualMatchmakingPool _pool = new();

    protected override CasualMatchmakingPool Pool => _pool;

    protected override void AddSeek(string userId) => _pool.AddSeek(userId);
}
