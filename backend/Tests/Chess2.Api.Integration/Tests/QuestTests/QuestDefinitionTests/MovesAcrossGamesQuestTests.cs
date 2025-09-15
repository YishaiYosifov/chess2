using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class MovesAcrossGamesQuestTests
{
    private readonly MovesAcrossGamesQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantMoveTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int requiredPlies)
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        for (int i = 0; i < 2; i++)
        {
            var snapshot = new GameQuestSnapshotFaker()
                .RuleFor(x => x.MoveHistory, new MoveFaker().Generate(requiredPlies))
                .Generate();

            instance.ApplySnapshot(snapshot);
        }

        instance.Progress.Should().Be(requiredPlies);
    }

    [Theory]
    [MemberData(nameof(VariantMoveTestData))]
    public void VariantProgress_does_not_count_zero_moves(int variantIdx, int _)
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var snapshot = new GameQuestSnapshotFaker().RuleFor(x => x.MoveHistory, []).Generate();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [InlineData(0, QuestDifficulty.Easy, 300)]
    [InlineData(1, QuestDifficulty.Medium, 600)]
    [InlineData(2, QuestDifficulty.Hard, 800)]
    public void QuestVariants_has_correct_metadata(
        int variantIdx,
        QuestDifficulty expectedDifficulty,
        int target
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        variant.Target.Should().Be(target);
        variant.Difficulty.Should().Be(expectedDifficulty);
    }

    public static TheoryData<int, int> VariantMoveTestData =>
        new()
        {
            { 0, 300 },
            { 1, 600 },
            { 2, 800 },
        };
}
