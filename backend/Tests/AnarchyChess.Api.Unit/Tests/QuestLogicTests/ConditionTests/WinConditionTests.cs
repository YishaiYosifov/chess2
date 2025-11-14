using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class WinConditionTests
{
    [Theory]
    [InlineData(GameResult.BlackWin, GameColor.White, false)]
    [InlineData(GameResult.WhiteWin, GameColor.White, true)]
    public void Evaluate_returns_expected_progress(
        GameResult result,
        GameColor playerColor,
        bool expectedResult
    )
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, playerColor)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(result).Generate())
            .Generate();

        WinCondition condition = new();

        bool progress = condition.Evaluate(snapshot);

        progress.Should().Be(expectedResult);
    }
}
