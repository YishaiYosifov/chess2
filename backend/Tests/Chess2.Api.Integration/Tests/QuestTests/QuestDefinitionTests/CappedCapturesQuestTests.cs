using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class CappedCapturesQuestTests
{
    private readonly CappedCapturesQuest _quest = new();
    private const int MinPlies = 30 * 2;

    [Theory]
    [MemberData(nameof(VariantCaptureCapTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int maxCaptures)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                whiteMoves: MoveFaker.Capture(GameColor.White).Generate(maxCaptures),
                totalPlies: MinPlies
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(VariantCaptureCapTestData))]
    public void VariantProgress_does_not_count_if_exceeding_capture_limit(
        int variantIdx,
        int maxCaptures
    )
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                whiteMoves: MoveFaker.Capture(GameColor.White).Generate(maxCaptures + 1),
                totalPlies: MinPlies
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(VariantCaptureCapTestData))]
    public void VariantProgress_does_not_count_if_too_short_game(int variantIdx, int _)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(totalPlies: 3)
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(VariantCaptureCapTestData))]
    public void VariantProgress_does_not_count_opponent_captures(int variantIdx, int maxCaptures)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                blackMoves: MoveFaker.Capture(GameColor.Black).Generate(maxCaptures),
                totalPlies: MinPlies
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(1);
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

    public static TheoryData<int, int> VariantCaptureCapTestData =>
        new()
        {
            { 0, 10 },
            { 1, 7 },
            { 2, 5 },
        };
}
