using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class TimeControlConditionTests
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void Evaluate_returns_true_for_matching_time_control(
        TimeControlSettings timeControl,
        TimeControlSettings actualTimeControl,
        bool expected
    )
    {
        var poolKey = new PoolKeyFaker().RuleFor(x => x.TimeControl, actualTimeControl);
        var gameState = new GameStateFaker().RuleFor(x => x.Pool, poolKey);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.FinalGameState, gameState)
            .Generate();
        new TimeControlCondition(timeControl).Evaluate(snapshot).Should().Be(expected);
    }

    public static TheoryData<TimeControlSettings, TimeControlSettings, bool> TestData =>
        new()
        {
            {
                new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 0),
                new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 0),
                true
            },
            {
                new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 0),
                new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 0),
                false
            },
            {
                new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 1),
                new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 0),
                false
            },
        };
}
