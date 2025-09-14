using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.MetricTests;

public class FirstOccurrenceMetricTests
{
    [Fact]
    public void EvaluateProgressMade_returns_total_moves_when_predicate_never_matches()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        FirstOccurrenceMetric metric = new((_, _) => false);

        int progressWhite = metric.Evaluate(snapshot with { PlayerColor = GameColor.White });
        int progressBlack = metric.Evaluate(snapshot with { PlayerColor = GameColor.Black });

        progressWhite.Should().Be(snapshot.MoveHistory.Count);
        progressBlack.Should().Be(snapshot.MoveHistory.Count);
    }

    [Fact]
    public void EvaluateProgressMade_returns_zero_when_first_move_matches()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        FirstOccurrenceMetric metric = new((move, _) => move == snapshot.MoveHistory[0]);

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
    }

    [Fact]
    public void EvaluateProgressMade_returns_index_of_first_matching_move_in_middle()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        FirstOccurrenceMetric metric = new((move, _) => move == snapshot.MoveHistory[3]);

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(3);
    }

    [Fact]
    public void EvaluateProgressMade_returns_index_of_first_matching_move_when_multiple_matches()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        FirstOccurrenceMetric metric = new(
            (move, _) => move == snapshot.MoveHistory[2] || move == snapshot.MoveHistory[3]
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(2);
    }

    [Fact]
    public void EvaluateProgressMade_iterates_over_all_moves()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        List<Move> iteratedMoves = [];
        FirstOccurrenceMetric metric = new(
            (move, _) =>
            {
                iteratedMoves.Add(move);
                return false;
            }
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(snapshot.MoveHistory.Count);
        iteratedMoves.Should().BeEquivalentTo(snapshot.MoveHistory);
    }
}
