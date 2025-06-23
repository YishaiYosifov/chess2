using Chess2.Api.Game.Models;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.Functional.Tests.GameFlowTests;

public abstract class BaseFunctionalGameFlowTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    protected const string MatchmakingHubPath = "/api/hub/matchmaking";
    protected const string SeekCasualMethod = "SeekCasualAsync";
    protected const string SeekRatedMethod = "SeekRatedAsync";

    protected static async Task<string> AssertPlayersMatchAsync(
        HubConnection conn1,
        HubConnection conn2,
        TimeControlSettings timeControl,
        string methodName
    )
    {
        await conn1.InvokeAsync(methodName, timeControl);
        await conn2.InvokeAsync(methodName, timeControl);

        var gameToken = await AssertMatchEstablishedAsync(conn1, conn2);
        return gameToken;
    }

    protected static async Task<string> AssertMatchEstablishedAsync(
        HubConnection conn1,
        HubConnection conn2
    )
    {
        var tcs1 = ListenForMatch(conn1);
        var tcs2 = ListenForMatch(conn2);

        var timeout = TimeSpan.FromSeconds(10);
        var gameToken1 = await tcs1.Task.WaitAsync(timeout, CT);
        var gameToken2 = await tcs2.Task.WaitAsync(timeout, CT);

        gameToken1.Should().NotBeNullOrEmpty().And.HaveLength(16).And.Be(gameToken2);

        return gameToken1;
    }

    protected static TaskCompletionSource<string> ListenForMatch(HubConnection conn)
    {
        var tcs = new TaskCompletionSource<string>();
        conn.On<string>("MatchFoundAsync", gameId => tcs.TrySetResult(gameId));
        return tcs;
    }
}
