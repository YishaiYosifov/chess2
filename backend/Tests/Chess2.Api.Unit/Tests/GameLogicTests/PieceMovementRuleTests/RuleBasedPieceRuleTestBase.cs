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

    protected IPieceMovementRule[] RuleMocks { get; }
    protected Move[] Moves { get; }

    protected RuleBasedPieceRuleTestBase()
    {
        Board.PlacePiece(Origin, Piece);

        var ruleMock1 = Substitute.For<IPieceMovementRule>();
        Move[] rule1Moves = [new(Origin, new("b2"), Piece), new(Origin, new("b3"), Piece)];
        ruleMock1.Evaluate(Board, Origin, Piece).Returns(rule1Moves);

        var ruleMock2 = Substitute.For<IPieceMovementRule>();
        Move[] rule2Moves = [new(Origin, new("c2"), Piece), new(Origin, new("c3"), Piece)];
        ruleMock2.Evaluate(Board, Origin, Piece).Returns(rule2Moves);

        RuleMocks = [ruleMock1, ruleMock2];
        Moves = [.. rule1Moves, .. rule2Moves];
    }
}
