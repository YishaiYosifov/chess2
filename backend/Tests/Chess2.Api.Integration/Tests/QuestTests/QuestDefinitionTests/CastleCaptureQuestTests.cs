using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class CastleCaptureQuestTests
{
    private readonly CastleCaptureQuest _quest = new();
    private readonly QuestVariant _variant;

    public CastleCaptureQuestTests()
    {
        _variant = _quest.Variants.First();
    }

    [Theory]
    [InlineData(SpecialMoveType.KingsideCastle)]
    [InlineData(SpecialMoveType.QueensideCastle)]
    public void VariantProgress_positive_snapshot(SpecialMoveType moveType)
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.SpecialMoveType, moveType)
                        .RuleFor(
                            x => x.Captures,
                            [new MoveCapture(PieceFactory.White(PieceType.Bishop), new())]
                        ),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _variant.Progressor.EvaluateProgressMade(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_without_castle_capture()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker().RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _variant.Progressor.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_on_loss()
    {
        var snapshot = GameQuestSnapshotFaker
            .Loss()
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle)
                        .RuleFor(
                            x => x.Captures,
                            [new MoveCapture(PieceFactory.White(PieceType.Bishop), new())]
                        ),
                    .. new MoveFaker().Generate(2),
                ]
            )
            .Generate();

        var progress = _variant.Progressor.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void QuestVariants_has_correct_metadata()
    {
        _variant.Target.Should().Be(2);
        _variant.Difficulty.Should().Be(QuestDifficulty.Medium);
    }
}
