using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class OpponentMoveOccurredConditionTests
{
    [Fact]
    public void Evaluate_returns_false_when_no_moves_match()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var condition = new OpponentMoveOccurredCondition(new PredicateMoveCondition(_ => false));

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_when_all_conditions_match()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var targetMove = snapshot.MoveHistory[1]; // opponent's first move for White

        var condition = new OpponentMoveOccurredCondition(
            new PredicateMoveCondition(move => true),
            new PredicateMoveCondition(move => move == targetMove)
        );

        condition.Evaluate(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_handles_empty_move_history()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleForMoves(totalPlies: 0).Generate();
        var condition = new OpponentMoveOccurredCondition(new PredicateMoveCondition(_ => true));

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(GameColor.White, 8, new[] { 1, 3, 5, 7 })] // white sees opponent moves at odd indices
    [InlineData(GameColor.Black, 8, new[] { 0, 2, 4, 6 })] // black sees opponent moves at even indices
    public void Evaluate_provides_only_opponent_moves(
        GameColor playerColor,
        int numOfMoves,
        int[] expectedIndices
    )
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleForMoves(totalPlies: numOfMoves)
            .Generate();

        List<Move> seenMoves = [];
        var condition = new OpponentMoveOccurredCondition(
            new PredicateMoveCondition(move =>
            {
                seenMoves.Add(move);
                return false;
            })
        );

        condition.Evaluate(snapshot);

        var expectedMoves = expectedIndices.Select(i => snapshot.MoveHistory[i]).ToList();
        seenMoves.Should().BeEquivalentTo(expectedMoves, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void Evaluate_returns_false_if_any_condition_fails()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();

        var condition1 = new PredicateMoveCondition(move => move == snapshot.MoveHistory[1]); // opponent’s move
        var condition2 = new PredicateMoveCondition(_ => false);

        var opponentCondition = new OpponentMoveOccurredCondition(condition1, condition2);

        opponentCondition.Evaluate(snapshot).Should().BeFalse();
    }
}
