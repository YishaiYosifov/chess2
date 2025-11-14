using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class MinimumGameLengthQuestTests
{
    private readonly MinimumGameLengthQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int gameLength)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.Black)
            .RuleForMoves(totalPlies: gameLength * 2)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.CreateInstance().ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_when_game_too_short(
        int variantIdx,
        int gameLength
    )
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.Black)
            .RuleForMoves(totalPlies: gameLength * 2 - 1)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.CreateInstance().ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_on_loss(int variantIdx, int gameLength)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForLoss(GameColor.White)
            .RuleForMoves(totalPlies: gameLength * 2)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.CreateInstance().ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [InlineData(0, QuestDifficulty.Easy)]
    [InlineData(1, QuestDifficulty.Medium)]
    [InlineData(2, QuestDifficulty.Hard)]
    public void QuestVariants_has_correct_metadata(
        int variantIdx,
        QuestDifficulty expectedDifficulty
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        variant.Target.Should().Be(1);
        variant.Difficulty.Should().Be(expectedDifficulty);
    }

    public static TheoryData<int, int> VariantMoveNumTestData =>
        new()
        {
            { 0, 80 },
            { 1, 100 },
            { 2, 130 },
        };
}
