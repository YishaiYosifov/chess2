using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class OwnMoveOccurredConditionTests
{
    [Fact]
    public void Evaluate_returns_false_when_no_moves_match()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        OwnMoveOccurredCondition condition = new((_, _) => false);

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_when_a_move_matches()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var targetMove = snapshot.MoveHistory[2];

        OwnMoveOccurredCondition condition = new((move, _) => move == targetMove);

        condition.Evaluate(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_handles_empty_move_history()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleForMoves(totalPlies: 0).Generate();

        OwnMoveOccurredCondition condition = new((_, _) => true);

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(GameColor.White, 8, new[] { 0, 2, 4, 6 })]
    [InlineData(GameColor.Black, 8, new[] { 1, 3, 5, 7 })]
    public void Evaluate_provides_only_player_moves(
        GameColor playerColor,
        int numOfMoves,
        int[] expectedIndices
    )
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleForMoves(totalPlies: numOfMoves)
            .Generate();

        List<Move> seenMoves = [];
        OwnMoveOccurredCondition condition = new(
            (move, _) =>
            {
                seenMoves.Add(move);
                return false;
            }
        );

        condition.Evaluate(snapshot);

        var expectedMoves = expectedIndices.Select(i => snapshot.MoveHistory[i]).ToList();
        seenMoves.Should().BeEquivalentTo(expectedMoves, opts => opts.WithStrictOrdering());
    }
}
