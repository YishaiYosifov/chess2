using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class MoveOccurredConditionTests
{
    [Fact]
    public void Evaluate_returns_false_when_no_moves_match()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        MoveOccurredCondition condition = new((_, _) => false);

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_when_any_move_matches()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var targetMove = snapshot.MoveHistory[2];

        MoveOccurredCondition condition = new((move, _) => move == targetMove);

        condition.Evaluate(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_handles_empty_move_history()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleFor(x => x.MoveHistory, []).Generate();
        MoveOccurredCondition condition = new((_, _) => true);

        condition.Evaluate(snapshot).Should().BeFalse();
    }
}
