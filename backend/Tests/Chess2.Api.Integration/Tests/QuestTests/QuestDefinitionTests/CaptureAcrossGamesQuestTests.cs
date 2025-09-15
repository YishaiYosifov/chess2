using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class CaptureAcrossGamesQuestTests
{
    private readonly CaptureAcrossGamesQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantCaptureTestData))]
    public void VariantProgress_accumulates_captures_across_games(
        int variantIdx,
        int requiredCaptures
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        for (int i = 0; i < 2; i++)
        {
            var snapshot = new GameQuestSnapshotFaker(GameColor.White)
                .RuleFor(
                    x => x.MoveHistory,
                    MoveFaker
                        .Capture(
                            GameColor.White,
                            captureType: PieceType.Pawn,
                            pieceType: PieceType.Pawn
                        )
                        .Generate((int)Math.Ceiling(requiredCaptures / 2.0))
                )
                .Generate();

            instance.ApplySnapshot(snapshot);
        }

        instance.Progress.Should().Be(requiredCaptures);
    }

    [Theory]
    [MemberData(nameof(VariantCaptureTestData))]
    public void VariantProgress_does_not_count_non_capture_moves(
        int variantIdx,
        int requiredCaptures
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.MoveHistory, new MoveFaker().Generate(requiredCaptures))
            .Generate();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [InlineData(0, QuestDifficulty.Easy, 20)]
    [InlineData(1, QuestDifficulty.Medium, 50)]
    [InlineData(2, QuestDifficulty.Hard, 80)]
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

    public static TheoryData<int, int> VariantCaptureTestData =>
        new()
        {
            { 0, 20 },
            { 1, 50 },
            { 2, 80 },
        };
}
