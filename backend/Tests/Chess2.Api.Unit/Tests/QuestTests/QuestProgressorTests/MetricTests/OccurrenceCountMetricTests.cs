using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Quests.QuestProgressors.Metrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.MetricTests;

public class OccurrenceCountMetricTests
{
    [Fact]
    public void EvaluateProgressMade_returns_zero_when_predicate_never_matches()
    {
        var moves = new MoveSnapshotFaker().Generate(5);
        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves).Generate();

        OccurrenceCountMetric metric = new((move, _, _) => false);

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(0);
    }

    [Fact]
    public void EvaluateProgressMade_counts_all_matching_moves()
    {
        var moves = new MoveSnapshotFaker().Generate(5);
        MoveSnapshot[] targetMoves = [moves[1], moves[3]];

        OccurrenceCountMetric metric = new((move, _, _) => targetMoves.Contains(move));

        int progress = metric.EvaluateProgressMade(
            new GameStateFaker().RuleFor(x => x.MoveHistory, moves).Generate(),
            GameColor.White
        );

        progress.Should().Be(2);
    }

    [Fact]
    public void EvaluateProgressMade_with_predicate_true_for_all_moves_returns_total_count()
    {
        var moves = new MoveSnapshotFaker().Generate(4);
        OccurrenceCountMetric metric = new((_, _, _) => true);

        int progress = metric.EvaluateProgressMade(
            new GameStateFaker().RuleFor(x => x.MoveHistory, moves).Generate(),
            GameColor.Black
        );

        progress.Should().Be(moves.Count);
    }

    [Fact]
    public void EvaluateProgressMade_iterrates_over_all_moves()
    {
        var moves = new MoveSnapshotFaker().Generate(5);

        List<Move> iteratedMoves = [];
        OccurrenceCountMetric metric = new(
            (move, _, _) =>
            {
                iteratedMoves.Add(move);
                return false;
            }
        );

        int progress = metric.EvaluateProgressMade(
            new GameStateFaker().RuleFor(x => x.MoveHistory, moves).Generate(),
            GameColor.White
        );

        progress.Should().Be(0);
        iteratedMoves.Should().BeEquivalentTo(moves);
    }
}
