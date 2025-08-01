using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public abstract class RuleBasedPieceRuleTestBase
{
    protected Piece Piece { get; } = PieceFactory.White();
    protected AlgebraicPoint Origin { get; } = new("a1");
    protected ChessBoard Board { get; } = new();

    protected Move Move1 { get; } = new(new("a1"), new("b2"), PieceFactory.White());
    protected Move Move2 { get; } = new(new("a1"), new("b3"), PieceFactory.White());
    protected Move Move3 { get; } = new(new("a1"), new("b4"), PieceFactory.White());

    protected IPieceMovementRule BaseRuleMock = Substitute.For<IPieceMovementRule>();

    protected RuleBasedPieceRuleTestBase()
    {
        Board.PlacePiece(Origin, Piece);
        BaseRuleMock.Evaluate(Board, Origin, Piece).Returns([Move1, Move2, Move3]);
    }
}
