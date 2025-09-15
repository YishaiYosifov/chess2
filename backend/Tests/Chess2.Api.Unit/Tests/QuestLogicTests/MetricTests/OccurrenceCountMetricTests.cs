using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class OccurrenceCountMetricTests
{
    [Fact]
    public void Evaluate_returns_zero_when_predicate_never_matches()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        OccurrenceCountMetric metric = new((move, _) => false);

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_all_matching_moves()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        HashSet<Move> targetMoves = [snapshot.MoveHistory[1], snapshot.MoveHistory[3]];

        OccurrenceCountMetric metric = new((move, _) => targetMoves.Contains(move));

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(2);
    }

    [Fact]
    public void Evaluate_with_predicate_true_for_all_moves_returns_total_count()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        OccurrenceCountMetric metric = new((_, _) => true);

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(snapshot.MoveHistory.Count);
    }

    [Fact]
    public void Evaluate_iterrates_over_all_moves()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        List<Move> iteratedMoves = [];
        OccurrenceCountMetric metric = new(
            (move, _) =>
            {
                iteratedMoves.Add(move);
                return false;
            }
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
        iteratedMoves.Should().BeEquivalentTo(iteratedMoves);
    }
}
