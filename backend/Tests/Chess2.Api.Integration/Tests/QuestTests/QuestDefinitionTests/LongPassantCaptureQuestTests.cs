using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class LongPassantCaptureQuestTests
{
    private readonly QuestInstance _quest;

    public LongPassantCaptureQuestTests()
    {
        _quest = new LongPassantCaptureQuest().Variants.First().CreateInstance();
    }

    [Fact]
    public void VariantProgress_positive_snapshot()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    MoveFaker
                        .Capture(
                            GameColor.White,
                            captureTypes: [PieceType.Pawn, PieceType.Pawn],
                            pieceType: PieceType.Pawn
                        )
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.EnPassant),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_when_done_by_opponent()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    MoveFaker
                        .Capture(
                            GameColor.Black,
                            captureTypes: [PieceType.Pawn, PieceType.Pawn],
                            pieceType: PieceType.Pawn
                        )
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.EnPassant),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_without_en_passant_capture()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    MoveFaker.Capture(
                        GameColor.White,
                        captureTypes: [PieceType.Pawn, PieceType.Pawn],
                        pieceType: PieceType.Pawn
                    ),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_if_capture_count_less_than_two()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    MoveFaker
                        .Capture(
                            GameColor.White,
                            captureType: PieceType.Pawn,
                            pieceType: PieceType.Pawn
                        )
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.EnPassant),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void QuestVariants_has_correct_metadata()
    {
        _quest.Target.Should().Be(1);
        _quest.Difficulty.Should().Be(QuestDifficulty.Easy);
    }
}
