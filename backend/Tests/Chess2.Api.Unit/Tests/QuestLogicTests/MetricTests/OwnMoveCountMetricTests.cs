using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class OwnMoveCountMetricTests
{
    [Fact]
    public void Evaluate_returns_zero_when_no_moves_match()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        OwnMoveCountMetric metric = new((_, _) => false);

        metric.Evaluate(snapshot).Should().Be(0);
    }

    [Theory]
    [InlineData(GameColor.White, new[] { 0, 2, 4 })]
    [InlineData(GameColor.Black, new[] { 1, 3, 5 })]
    public void Evaluate_counts_only_matching_player_moves(GameColor playerColor, int[] indices)
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleFor(x => x.MoveHistory, f => new MoveFaker().Generate(6))
            .Generate();

        var targetMoves = indices.Select(i => snapshot.MoveHistory[i]).ToHashSet();

        OwnMoveCountMetric metric = new((move, _) => targetMoves.Contains(move));

        metric.Evaluate(snapshot).Should().Be(indices.Length);
    }

    [Fact]
    public void Evaluate_returns_zero_for_empty_history()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleFor(x => x.MoveHistory, []).Generate();

        OwnMoveCountMetric metric = new((_, _) => true);

        metric.Evaluate(snapshot).Should().Be(0);
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
            .RuleFor(x => x.MoveHistory, f => new MoveFaker().Generate(numOfMoves))
            .Generate();

        List<Move> seenMoves = [];
        OwnMoveCountMetric metric = new(
            (move, _) =>
            {
                seenMoves.Add(move);
                return false;
            }
        );

        metric.Evaluate(snapshot);

        var expectedMoves = expectedIndices.Select(i => snapshot.MoveHistory[i]).ToList();
        seenMoves.Should().BeEquivalentTo(expectedMoves, opts => opts.WithStrictOrdering());
    }
}
