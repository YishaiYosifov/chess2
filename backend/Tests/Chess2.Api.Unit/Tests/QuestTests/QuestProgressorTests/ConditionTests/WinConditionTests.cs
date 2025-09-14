using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Quests.QuestMetrics;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.ConditionTests;

public class WinConditionTests
{
    [Theory]
    [InlineData(GameResult.BlackWin, GameColor.White, 0)] // loss
    [InlineData(GameResult.WhiteWin, GameColor.White, 1)] // win
    public void EvaluateProgressMade_returns_expected_progress(
        GameResult result,
        GameColor playerColor,
        int expectedProgress
    )
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, playerColor)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(result).Generate())
            .Generate();

        var winCondition = new WinCondition(inner: null);

        int progress = winCondition.EvaluateProgressMade(snapshot);

        progress.Should().Be(expectedProgress);
    }

    [Theory]
    [InlineData(GameResult.BlackWin, GameColor.White, false)] // loss
    [InlineData(GameResult.WhiteWin, GameColor.White, true)] // win
    public void EvaluateProgressMade_returns_correct_progress_when_inner_is_provided(
        GameResult result,
        GameColor playerColor,
        bool expectProgress
    )
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, playerColor)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(result).Generate())
            .Generate();

        var inner = Substitute.For<IQuestMetric>();
        inner.Evaluate(snapshot).Returns(5);

        var winCondition = new WinCondition(inner);

        int progress = winCondition.EvaluateProgressMade(snapshot);

        if (expectProgress)
            progress.Should().Be(5);
        else
            progress.Should().Be(0);
    }
}
