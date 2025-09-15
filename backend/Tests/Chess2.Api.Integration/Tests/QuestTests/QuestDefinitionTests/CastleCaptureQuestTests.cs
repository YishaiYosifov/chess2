using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

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
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.Piece, PieceFactory.White(PieceType.King))
                        .RuleFor(x => x.SpecialMoveType, moveType)
                        .RuleFor(
                            x => x.Captures,
                            [
                                new MoveCapture(
                                    PieceFactory.White(PieceType.Bishop),
                                    new AlgebraicPoint()
                                ),
                            ]
                        ),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_without_castle_capture()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.Piece, PieceFactory.White(PieceType.King))
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_on_loss()
    {
        var snapshot = GameQuestSnapshotFaker
            .Loss()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.Piece, PieceFactory.White(PieceType.King))
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle)
                        .RuleFor(
                            x => x.Captures,
                            [new MoveCapture(PieceFactory.White(PieceType.Bishop), new())]
                        ),
                    .. new MoveFaker().Generate(2),
                ]
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
