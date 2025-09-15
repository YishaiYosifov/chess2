using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class PawnPromotionsAcrossGamesQuestTests
{
    private readonly PawnPromotionsAcrossGamesQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantPromotionTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int requiredPromotions)
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        int perSnapshot = (int)Math.Ceiling(requiredPromotions / 2.0);
        for (int i = 0; i < 2; i++)
        {
            var snapshot = new GameQuestSnapshotFaker(GameColor.White)
                .RuleFor(
                    x => x.MoveHistory,
                    new MoveFaker(GameColor.White)
                        .RuleFor(m => m.PromotesTo, PieceType.Queen)
                        .Generate(perSnapshot)
                )
                .Generate();

            instance.ApplySnapshot(snapshot);
        }

        instance.Progress.Should().Be(requiredPromotions);
    }

    [Theory]
    [MemberData(nameof(VariantPromotionTestData))]
    public void VariantProgress_does_not_count_opponent_promotions(
        int variantIdx,
        int requiredPromotions
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                new MoveFaker(GameColor.Black)
                    .RuleFor(m => m.PromotesTo, PieceType.Queen)
                    .Generate(requiredPromotions)
            )
            .Generate();

        int progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [InlineData(0, QuestDifficulty.Easy, 5)]
    [InlineData(1, QuestDifficulty.Medium, 10)]
    [InlineData(2, QuestDifficulty.Hard, 20)]
    public void QuestVariants_has_correct_metadata(
        int variantIdx,
        QuestDifficulty expectedDifficulty,
        int target
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        variant.Target.Should().Be(target);
        variant.Difficulty.Should().Be(expectedDifficulty);
        variant.Description.Should().Contain(target.ToString());
    }

    public static TheoryData<int, int> VariantPromotionTestData =>
        new()
        {
            { 0, 5 },
            { 1, 10 },
            { 2, 20 },
        };
}
