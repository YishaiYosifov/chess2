using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class OwnMoveCountMetricTests
{
    [Fact]
    public void Evaluate_returns_zero_when_no_moves_match()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        OwnMoveCountMetric metric = new(new PredicateMoveCondition(_ => false));

        metric.Evaluate(snapshot).Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_count_for_matching_conditions()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleForMoves(totalPlies: 5)
            .Generate();
        HashSet<Move> targetMoves = [snapshot.MoveHistory[0], snapshot.MoveHistory[2]];

        OwnMoveCountMetric condition = new(
            new PredicateMoveCondition(move => true),
            new PredicateMoveCondition(targetMoves.Contains)
        );

        condition.Evaluate(snapshot).Should().Be(2);
    }

    [Theory]
    [InlineData(GameColor.White, new[] { 0, 2, 4 })]
    [InlineData(GameColor.Black, new[] { 1, 3, 5 })]
    public void Evaluate_counts_only_matching_player_moves(GameColor playerColor, int[] indices)
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleForMoves(totalPlies: 6)
            .Generate();

        var targetMoves = indices.Select(i => snapshot.MoveHistory[i]).ToHashSet();

        OwnMoveCountMetric metric = new(new PredicateMoveCondition(targetMoves.Contains));

        metric.Evaluate(snapshot).Should().Be(indices.Length);
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

        List<Move> seenMoves = [];
        OwnMoveCountMetric metric = new(
            new PredicateMoveCondition(move =>
            {
                seenMoves.Add(move);
                return false;
            })
        );

        metric.Evaluate(snapshot);

        var expectedMoves = expectedIndices.Select(i => snapshot.MoveHistory[i]).ToList();
        seenMoves.Should().BeEquivalentTo(expectedMoves, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void Evaluate_returns_0_if_any_condition_fails()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleForMoves(totalPlies: 5)
            .Generate();

        OwnMoveCountMetric condition = new(
            new PredicateMoveCondition(move => false),
            new PredicateMoveCondition(move => move == snapshot.MoveHistory[2])
        );

        condition.Evaluate(snapshot).Should().Be(0);
    }
}
