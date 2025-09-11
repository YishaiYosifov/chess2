using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.QuestProgressors.Metrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.MetricTests;

public class GameLengthMetricTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void EvaluateProgressMade_returns_number_of_moves(int moveCount)
    {
        var snapshot = new GameStateFaker().RuleFor(
            x => x.MoveHistory,
            new MoveSnapshotFaker().Generate(moveCount)
        );
        GameLengthMetric progressor = new();

        int progressWhite = progressor.EvaluateProgressMade(snapshot, GameColor.White);
        int progressBlack = progressor.EvaluateProgressMade(snapshot, GameColor.Black);

        progressWhite.Should().Be(moveCount);
        progressBlack.Should().Be(moveCount);
    }
}
