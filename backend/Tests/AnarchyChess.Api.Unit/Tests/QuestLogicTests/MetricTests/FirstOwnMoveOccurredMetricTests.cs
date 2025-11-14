using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class FirstOwnMoveOccurredMetricTests
{
    [Fact]
    public void Evaluate_returns_max_value_if_any_condition_fails()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        var metric = new FirstOwnMoveOccurredMetric(
            new PredicateMoveCondition(_ => false),
            new PredicateMoveCondition(move => move == snapshot.MoveHistory[2])
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Evaluate_returns_zero_when_first_own_move_matches()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var metric = new FirstOwnMoveOccurredMetric(
            new PredicateMoveCondition(move => move == snapshot.MoveHistory[0])
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_index_of_first_matching_move_in_middle()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var targetMove = snapshot.MoveHistory[2];
        var metric = new FirstOwnMoveOccurredMetric(
            new PredicateMoveCondition(move => move == targetMove),
            new PredicateMoveCondition(move => true)
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(2);
    }

    [Theory]
    [InlineData(GameColor.White, 8, new[] { 0, 2, 4, 6 })]
    [InlineData(GameColor.Black, 8, new[] { 1, 3, 5, 7 })]
    public void Evaluate_checks_all_and_only_player_moves(
        GameColor playerColor,
        int numOfMoves,
        int[] expectedIndices
    )
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleForMoves(totalPlies: numOfMoves)
            .Generate();

        var seenMoves = new List<Move>();

        var metric = new FirstOwnMoveOccurredMetric(
            new PredicateMoveCondition(move =>
            {
                seenMoves.Add(move);
                return false;
            })
        );

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(int.MaxValue);

        var expectedMoves = expectedIndices.Select(i => snapshot.MoveHistory[i]).ToList();
        seenMoves.Should().BeEquivalentTo(expectedMoves, opts => opts.WithStrictOrdering());
    }
}
