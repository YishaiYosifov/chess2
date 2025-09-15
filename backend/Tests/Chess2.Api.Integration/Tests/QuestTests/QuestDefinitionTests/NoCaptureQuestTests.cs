using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class NoCaptureQuestTests
{
    private readonly NoCaptureQuest _quest = new();

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_positive_snapshot(int variantIdx, int numOfMoves)
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(
                x => x.MoveHistory,
                [
                    .. new MoveFaker().Generate(numOfMoves * 2),
                    new MoveFaker().RuleFor(
                        x => x.Captures,
                        [new MoveCapture(PieceFactory.White(), new())]
                    ),
                ]
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_with_capture_before_allowed(
        int variantIdx,
        int numOfMoves
    )
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker().RuleFor(
                        x => x.Captures,
                        [new MoveCapture(PieceFactory.White(), new())]
                    ),
                    .. new MoveFaker().Generate(numOfMoves * 2),
                ]
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(VariantMoveNumTestData))]
    public void VariantProgress_does_not_progress_on_loss(int variantIdx, int numOfMoves)
    {
        var snapshot = GameQuestSnapshotFaker
            .Loss()
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker().RuleFor(
                        x => x.Captures,
                        [new MoveCapture(PieceFactory.White(), new())]
                    ),
                    .. new MoveFaker().Generate(numOfMoves * 2),
                ]
            )
            .Generate();

        var variant = _quest.Variants.ElementAt(variantIdx);
        var progress = variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
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
        variant.Target.Should().Be(5);
        variant.Difficulty.Should().Be(expectedDifficulty);
    }

    public static TheoryData<int, int> VariantMoveNumTestData =>
        new()
        {
            { 0, 7 },
            { 1, 11 },
            { 2, 15 },
        };
}
