using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class WinInQuestTests
{
    private readonly WinInQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int maxMoves)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.Black)
            .RuleForMoves(totalPlies: maxMoves * 2)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.CreateInstance().ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_when_too_long(int variantIdx, int maxMoves)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.Black)
            .RuleForMoves(totalPlies: maxMoves * 2 + 2)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.CreateInstance().ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_on_loss(int variantIdx, int maxMoves)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForLoss(GameColor.White)
            .RuleForMoves(totalPlies: maxMoves * 2)
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
        variant.Target.Should().Be(3);
        variant.Difficulty.Should().Be(expectedDifficulty);
    }

    public static TheoryData<int, int> VariantMoveNumTestData =>
        new()
        {
            { 0, 35 },
            { 1, 25 },
            { 2, 15 },
        };
}
