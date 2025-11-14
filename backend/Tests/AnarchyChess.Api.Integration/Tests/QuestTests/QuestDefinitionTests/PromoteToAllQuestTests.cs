using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using FluentAssertions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class PromoteToAllQuestTests
{
    private readonly PromoteToAllQuest _quest = new();

    [Fact]
    public void QuestVariant_has_correct_metadata()
    {
        var variant = _quest.Variants.Single();
        variant.Difficulty.Should().Be(QuestDifficulty.Medium);
        variant.Target.Should().Be(GameLogicConstants.PromotablePieces.Count);
    }

    [Fact]
    public void Variant_uses_progressive_unique_promotions_metric()
    {
        var variant = _quest.Variants.Single();
        var progressors = variant.Progressors?.Invoke();

        progressors
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<ProgressiveUniquePromotionsMetric>();
    }
}
