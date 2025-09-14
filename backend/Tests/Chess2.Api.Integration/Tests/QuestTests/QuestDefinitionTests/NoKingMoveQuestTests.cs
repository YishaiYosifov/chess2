using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class NoKingMoveQuestTests
{
    private readonly NoKingMoveQuest _quest = new();
    private readonly QuestVariant _variant;
    private const int MinMoves = 30 * 2;

    public NoKingMoveQuestTests()
    {
        _variant = _quest.Variants.First();
    }

    [Fact]
    public void VariantProgress_positive_snapshot()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    .. new MoveFaker()
                        .RuleFor(x => x.Piece, PieceFactory.White(PieceType.Horsey))
                        .Generate(MinMoves),
                ]
            )
            .Generate();

        var progress = _variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_if_king_moves()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(
                x => x.MoveHistory,
                (f, x) =>

                    [
                        new MoveFaker()
                            .RuleFor(
                                x => x.Piece,
                                new PieceFaker(
                                    color: x.PlayerColor,
                                    piece: PieceType.King
                                ).Generate()
                            )
                            .Generate(),
                        .. new MoveFaker().Generate(MinMoves),
                    ]
            )
            .Generate();

        var progress = _variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_if_too_short()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.MoveHistory, [.. new MoveFaker().Generate(MinMoves - 1)])
            .Generate();

        var progress = _variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_on_loss()
    {
        var snapshot = GameQuestSnapshotFaker
            .Loss()
            .RuleFor(x => x.MoveHistory, [.. new MoveFaker().Generate(MinMoves)])
            .Generate();

        var progress = _variant.Progressors.EvaluateProgressMade(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void QuestVariants_has_correct_metadata()
    {
        _variant.Target.Should().Be(1);
        _variant.Difficulty.Should().Be(QuestDifficulty.Medium);
    }
}
