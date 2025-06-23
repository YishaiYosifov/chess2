using Chess2.Api.Game.Models;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.Functional.Tests.GameFlowTests;

public abstract class BaseFunctionalGameFlowTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    protected static async Task AssertPlayersMatchAsync(
        HubConnection conn1,
        HubConnection conn2,
        string methodName,
        TimeControlSettings timeControl
    )
    {
        await conn1.InvokeAsync(methodName, timeControl);
        await conn2.InvokeAsync(methodName, timeControl);

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    protected static async Task AssertMatchEstablishedAsync(
        HubConnection conn1,
        HubConnection conn2
    )
    {
        var tcs1 = ListenForMatch(conn1);
        var tcs2 = ListenForMatch(conn2);

        var timeout = TimeSpan.FromSeconds(10);
        var gameId1 = await tcs1.Task.WaitAsync(timeout, CT);
        var gameId2 = await tcs2.Task.WaitAsync(timeout, CT);

        gameId1.Should().NotBeNullOrEmpty().And.HaveLength(16).And.Be(gameId2);
    }

    protected static TaskCompletionSource<string> ListenForMatch(HubConnection conn)
    {
        var tcs = new TaskCompletionSource<string>();
        conn.On<string>("MatchFoundAsync", gameId => tcs.TrySetResult(gameId));
        return tcs;
    }
}
