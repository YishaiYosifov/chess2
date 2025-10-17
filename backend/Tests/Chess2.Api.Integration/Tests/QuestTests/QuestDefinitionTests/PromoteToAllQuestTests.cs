﻿using Chess2.Api.GameLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.QuestLogic.QuestMetrics;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

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
