using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class NotConditionTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Evaluate_negates_inner_result(bool innerResult, bool expected)
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var inner = Substitute.For<IQuestCondition>();
        inner.Evaluate(snapshot).Returns(innerResult);

        NotCondition condition = new(inner);

        condition.Evaluate(snapshot).Should().Be(expected);
    }
}
