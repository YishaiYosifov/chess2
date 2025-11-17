using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class TimeUsedMsMetricTests
{
    [Fact]
    public void Evaluate_returns_correct_time_used_for_white()
    {
        var snapshot = CreateSnapshot(
            playerColor: GameColor.White,
            timeControl: new(BaseSeconds: 60, IncrementSeconds: 5),
            whiteClock: 30_000,
            blackClock: 45_000,
            totalPlies: 4
        );

        TimeUsedMsMetric metric = new();
        int result = metric.Evaluate(snapshot);

        // base = 60 * 1000 = 60,000
        // timeLeft = 30,000
        // playerMoves = 2
        // increment = 5 * 1,000 * 2 = 10,000
        // total = 60,000 - 30,000 + 10,000 = 40,000
        result.Should().Be(40000);
    }

    [Fact]
    public void Evaluate_returns_correct_time_used_for_black()
    {
        var snapshot = CreateSnapshot(
            playerColor: GameColor.Black,
            timeControl: new(BaseSeconds: 120, IncrementSeconds: 10),
            whiteClock: 30_000,
            blackClock: 20_000,
            totalPlies: 5
        );

        TimeUsedMsMetric metric = new();
        int result = metric.Evaluate(snapshot);

        // base = 120 * 1000 = 120,000
        // timeLeft = 20,000
        // playerMoves = (5 + 1) / 2 = 3
        // increment = 10 * 1,000 * 3 = 30,000
        // total = 120,000 - 20,000 + 30,000 = 130,000
        result.Should().Be(130000);
    }

    [Fact]
    public void Evaluate_handles_zero_moves()
    {
        var snapshot = CreateSnapshot(
            GameColor.White,
            timeControl: new(BaseSeconds: 90, IncrementSeconds: 0),
            whiteClock: 90_000,
            blackClock: 90_000,
            totalPlies: 0
        );

        TimeUsedMsMetric metric = new();

        int result = metric.Evaluate(snapshot);

        result.Should().Be(0);
    }

    private static GameQuestSnapshot CreateSnapshot(
        GameColor playerColor,
        TimeControlSettings timeControl,
        double whiteClock,
        double blackClock,
        int totalPlies
    )
    {
        ClockSnapshot clock = new(
            WhiteClock: whiteClock,
            BlackClock: blackClock,
            LastUpdated: 0,
            IsFrozen: true
        );
        var poolKey = new PoolKeyFaker().RuleFor(x => x.TimeControl, timeControl);
        var gameState = new GameStateFaker()
            .RuleFor(x => x.Clocks, clock)
            .RuleFor(x => x.Pool, poolKey);

        return new GameQuestSnapshotFaker(playerColor)
            .RuleFor(x => x.FinalGameState, gameState)
            .RuleForMoves(totalPlies: totalPlies)
            .Generate();
    }
}
