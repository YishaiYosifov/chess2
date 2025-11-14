using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class CastleCaptureQuestTests
{
    private readonly QuestInstance _quest;

    public CastleCaptureQuestTests()
    {
        _quest = new CastleCaptureQuest().Variants.First().CreateInstance();
    }

    [Theory]
    [InlineData(SpecialMoveType.KingsideCastle)]
    [InlineData(SpecialMoveType.QueensideCastle)]
    public void VariantProgress_positive_snapshot(SpecialMoveType moveType)
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                whiteMoves: MoveFaker
                    .Capture(
                        GameColor.White,
                        captureType: PieceType.Bishop,
                        pieceType: PieceType.King
                    )
                    .RuleFor(x => x.SpecialMoveType, moveType)
                    .Generate(1)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_when_done_by_opponent()
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                blackMoves: MoveFaker
                    .Capture(
                        GameColor.Black,
                        captureType: PieceType.Bishop,
                        pieceType: PieceType.King
                    )
                    .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle)
                    .Generate(1)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_without_castle_capture()
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForWin(GameColor.White)
            .RuleForMoves(
                whiteMoves: new MoveFaker(GameColor.White, PieceType.King)
                    .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle)
                    .Generate(1)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_on_loss()
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleForLoss(GameColor.White)
            .RuleForMoves(
                whiteMoves: MoveFaker
                    .Capture(
                        GameColor.White,
                        captureType: PieceType.Bishop,
                        pieceType: PieceType.King
                    )
                    .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle)
                    .Generate(1)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void QuestVariants_has_correct_metadata()
    {
        _quest.Target.Should().Be(2);
        _quest.Difficulty.Should().Be(QuestDifficulty.Medium);
    }
}
