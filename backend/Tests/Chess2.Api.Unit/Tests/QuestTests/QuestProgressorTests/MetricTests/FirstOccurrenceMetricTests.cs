using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Quests.QuestProgressors.Metrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.MetricTests;

public class FirstOccurrenceMetricTests
{
    [Fact]
    public void EvaluateProgressMade_returns_total_moves_when_predicate_never_matches()
    {
        var moves = new MoveSnapshotFaker().Generate(5);
        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        FirstOccurrenceMetric metric = new((move, _, _) => false);

        int progressWhite = metric.EvaluateProgressMade(snapshot, GameColor.White);
        int progressBlack = metric.EvaluateProgressMade(snapshot, GameColor.Black);

        progressWhite.Should().Be(5);
        progressBlack.Should().Be(5);
    }

    [Fact]
    public void EvaluateProgressMade_returns_zero_when_first_move_matches()
    {
        var firstMove = new MoveSnapshotFaker().Generate();
        var moves = new List<MoveSnapshot> { firstMove }
            .Concat(new MoveSnapshotFaker().Generate(4))
            .ToList();

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        FirstOccurrenceMetric metric = new((move, _, _) => move == firstMove);

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(0);
    }

    [Fact]
    public void EvaluateProgressMade_returns_index_of_first_matching_move_in_middle()
    {
        var moves = new MoveSnapshotFaker().Generate(3);
        var targetMove = new MoveSnapshotFaker().Generate();
        moves.Add(targetMove);
        moves.AddRange(new MoveSnapshotFaker().Generate(2));

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        FirstOccurrenceMetric metric = new((move, _, _) => move == targetMove);

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(3);
    }

    [Fact]
    public void EvaluateProgressMade_returns_index_of_first_matching_move_when_multiple_matches()
    {
        var moves = new MoveSnapshotFaker().Generate(2);
        var firstMatch = new MoveSnapshotFaker().Generate();
        var secondMatch = new MoveSnapshotFaker().Generate();

        moves.Add(firstMatch);
        moves.AddRange(new MoveSnapshotFaker().Generate(1));
        moves.Add(secondMatch);

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        FirstOccurrenceMetric metric = new(
            (move, _, _) => move == firstMatch || move == secondMatch
        );

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.Black);

        progress.Should().Be(2);
    }

    [Fact]
    public void EvaluateProgressMade_iterates_over_all_moves()
    {
        var moves = new MoveSnapshotFaker().Generate(5);
        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        List<MoveSnapshot> iteratedMoves = [];
        FirstOccurrenceMetric metric = new(
            (move, _, _) =>
            {
                iteratedMoves.Add(move);
                return false;
            }
        );

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(moves.Count);
        iteratedMoves.Should().BeEquivalentTo(moves);
    }
}
