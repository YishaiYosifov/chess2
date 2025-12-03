using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class GameLengthMetricTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Evaluate_returns_number_of_moves(int moveCount)
    {
        var snapshot = new GameQuestSnapshotFaker().RuleForMoves(totalPlies: moveCount).Generate();
        MoveCountMetric progressor = new();

        int progressWhite = progressor.Evaluate(snapshot with { PlayerColor = GameColor.White });
        int progressBlack = progressor.Evaluate(snapshot with { PlayerColor = GameColor.Black });

        progressWhite.Should().Be(moveCount / 2);
        progressBlack.Should().Be(moveCount / 2);
    }
}
