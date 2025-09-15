using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public abstract class NoPieceMoveQuestTestBase<TQuest>(
    PieceType forbiddenPiece,
    PieceType allowedPiece,
    int minMoves,
    QuestDifficulty expectedDifficulty,
    int target
)
    where TQuest : IQuestDefinition, new()
{
    private readonly QuestInstance _quest = new TQuest().Variants.First().CreateInstance();

    private readonly int _minMoves = minMoves;
    private readonly QuestDifficulty _expectedDifficulty = expectedDifficulty;
    private readonly int _target = target;
    private readonly PieceType _forbiddenPiece = forbiddenPiece;
    private readonly PieceType _allowedPiece = allowedPiece;

    [Fact]
    public void VariantProgress_positive_snapshot()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                new MoveFaker()
                    .RuleFor(x => x.Piece, PieceFactory.White(_allowedPiece))
                    .Generate(_minMoves)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(1);
    }

    [Fact]
    public void VariantProgress_does_not_progress_if_forbidden_piece_moves()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker()
                        .RuleFor(x => x.Piece, PieceFactory.White(_forbiddenPiece))
                        .Generate(),
                    .. new MoveFaker().Generate(_minMoves),
                ]
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void VariantProgress_does_not_progress_if_too_short()
    {
        var snapshot = GameQuestSnapshotFaker
            .Win()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                new MoveFaker()
                    .RuleFor(x => x.Piece, PieceFactory.White(_allowedPiece))
                    .Generate(_minMoves - 1)
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
                new MoveFaker()
                    .RuleFor(x => x.Piece, PieceFactory.White(_allowedPiece))
                    .Generate(_minMoves)
            )
            .Generate();

        var progress = _quest.ApplySnapshot(snapshot);
        progress.Should().Be(0);
    }

    [Fact]
    public void QuestVariants_has_correct_metadata()
    {
        _quest.Target.Should().Be(_target);
        _quest.Difficulty.Should().Be(_expectedDifficulty);
    }
}
