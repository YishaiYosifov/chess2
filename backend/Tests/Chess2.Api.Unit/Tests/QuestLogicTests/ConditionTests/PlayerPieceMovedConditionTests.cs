using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class PlayerPieceMovedConditionTests
{
    [Fact]
    public void Evaluate_returns_true_when_player_moved_target_piece()
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .Generate();

        PlayerPieceMovedCondition condition = new(snapshot.MoveHistory[2].Piece.Type);

        condition.Evaluate(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_when_player_has_not_moved_target_piece()
    {
        var move = new MoveFaker()
            .RuleFor(x => x.Piece, PieceFactory.White(PieceType.Horsey))
            .Generate();
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.MoveHistory, [move])
            .Generate();

        PlayerPieceMovedCondition condition = new(PieceType.King);

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_when_opponent_moved_target_piece()
    {
        var move = new MoveFaker()
            .RuleFor(x => x.Piece, PieceFactory.Black(PieceType.King))
            .Generate();
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.MoveHistory, [move])
            .Generate();

        PlayerPieceMovedCondition condition = new(PieceType.King);

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_when_no_moves_for_player()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleFor(x => x.MoveHistory, []).Generate();

        PlayerPieceMovedCondition condition = new(PieceType.Horsey);

        condition.Evaluate(snapshot).Should().BeFalse();
    }
}
