using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class CheckerHopQuestTests
{
    private readonly CheckerHopQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantHopCaptureTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int piecesToCapture)
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleForMoves(
                whiteMoves: MoveFaker
                    .Capture(
                        GameColor.White,
                        pieceType: PieceType.Checker,
                        captureTypes: Enumerable
                            .Range(0, piecesToCapture)
                            .Select(x => PieceType.Pawn)
                    )
                    .Generate(1)
            )
            .Generate();

        instance.ApplySnapshot(snapshot);
        instance.Progress.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(VariantHopCaptureTestData))]
    public void VariantProgress_does_not_count_non_checker_capture_moves(
        int variantIdx,
        int piecesToCapture
    )
    {
        var variant = _quest.Variants.ElementAt(variantIdx);
        var instance = variant.CreateInstance();

        var snapshot = new GameQuestSnapshotFaker()
            .RuleForMoves(
                whiteMoves: MoveFaker
                    .Capture(
                        GameColor.White,
                        pieceType: PieceType.Knook,
                        captureTypes: Enumerable
                            .Range(0, piecesToCapture)
                            .Select(x => PieceType.Pawn)
                    )
                    .Generate(1)
            )
            .Generate();

        var progress = instance.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [InlineData(0, QuestDifficulty.Easy, 1)]
    [InlineData(1, QuestDifficulty.Medium, 1)]
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

    public static TheoryData<int, int> VariantHopCaptureTestData => new() { { 0, 2 }, { 1, 3 } };
}
