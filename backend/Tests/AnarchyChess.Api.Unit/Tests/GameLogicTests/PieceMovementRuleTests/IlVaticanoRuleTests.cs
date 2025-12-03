using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class IlVaticanoRuleTests
{
    [Theory]
    [ClassData(typeof(ValidIlVaticanoTestData))]
    public void Evaluate_allows_il_vaticano_when_conditions_are_met(
        AlgebraicPoint origin,
        AlgebraicPoint intermediate1,
        AlgebraicPoint intermediate2,
        AlgebraicPoint partnerSquare,
        Offset stepOffset
    )
    {
        ChessBoard board = new();
        Piece movingPiece = PieceFactory.White(PieceType.Bishop);
        Piece partnerPiece = PieceFactory.White(PieceType.Bishop);
        Piece intermediatePiece1 = PieceFactory.Black();
        Piece intermediatePiece2 = PieceFactory.Black();

        board.PlacePiece(origin, movingPiece);
        board.PlacePiece(intermediate1, intermediatePiece1);
        board.PlacePiece(intermediate2, intermediatePiece2);
        board.PlacePiece(partnerSquare, partnerPiece);

        IlVaticanoRule rule = new(stepOffset);

        var result = rule.Evaluate(board, origin, movingPiece).ToList();

        List<MoveCapture> expectedCaptures =
        [
            new MoveCapture(intermediatePiece1, intermediate1),
            new MoveCapture(intermediatePiece2, intermediate2),
        ];
        List<AlgebraicPoint> triggers = [intermediate1, intermediate2];
        MoveSideEffect sideEffect = new(From: partnerSquare, To: origin, partnerPiece);

        Move expected = new(
            from: origin,
            to: partnerSquare,
            piece: movingPiece,
            triggerSquares: triggers,
            captures: expectedCaptures,
            sideEffects: [sideEffect],
            specialMoveType: SpecialMoveType.IlVaticano
        );

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
    }

    private static ChessBoard CreateIlVaticanoBoard(
        AlgebraicPoint origin,
        Piece movingPiece,
        params (AlgebraicPoint pos, Piece? piece)[] additionalPieces
    )
    {
        var board = new ChessBoard();
        board.PlacePiece(origin, movingPiece);

        foreach (var (pos, piece) in additionalPieces)
        {
            if (piece is not null)
                board.PlacePiece(pos, piece);
        }

        return board;
    }

    [Fact]
    public void Evaluate_returns_nothing_if_partner_is_missing()
    {
        var board = CreateIlVaticanoBoard(
            new("d4"),
            PieceFactory.White(PieceType.Bishop),
            (new("e4"), PieceFactory.Black()),
            (new("f4"), PieceFactory.Black())
        // partner is missing
        );

        var rule = new IlVaticanoRule(new Offset(1, 0));
        rule.Evaluate(board, new("d4"), PieceFactory.White(PieceType.Bishop)).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_partner_has_wrong_type()
    {
        var board = CreateIlVaticanoBoard(
            new("d4"),
            PieceFactory.White(PieceType.Bishop),
            (new("e4"), PieceFactory.Black()),
            (new("f4"), PieceFactory.Black()),
            (new("g4"), new Piece(PieceType.Rook, GameColor.White))
        );

        var rule = new IlVaticanoRule(new Offset(1, 0));
        rule.Evaluate(board, new("d4"), PieceFactory.White(PieceType.Bishop)).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_partner_has_wrong_color()
    {
        var board = CreateIlVaticanoBoard(
            new("d4"),
            PieceFactory.White(PieceType.Bishop),
            (new("e4"), PieceFactory.Black()),
            (new("f4"), PieceFactory.Black()),
            (new("g4"), PieceFactory.Black(PieceType.Bishop))
        );

        var rule = new IlVaticanoRule(new Offset(1, 0));
        rule.Evaluate(board, new("d4"), PieceFactory.White(PieceType.Bishop)).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_intermediate_is_empty()
    {
        var board = CreateIlVaticanoBoard(
            new("d4"),
            PieceFactory.White(PieceType.Bishop),
            (new("e4"), null), // intermediate1 is empty
            (new("f4"), PieceFactory.Black()),
            (new("g4"), PieceFactory.White(PieceType.Bishop))
        );

        var rule = new IlVaticanoRule(new Offset(1, 0));
        rule.Evaluate(board, new("d4"), PieceFactory.White(PieceType.Bishop)).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_intermediate_has_friendly_piece()
    {
        var board = CreateIlVaticanoBoard(
            new("d4"),
            PieceFactory.White(PieceType.Bishop),
            (new("e4"), PieceFactory.White()),
            (new("f4"), PieceFactory.Black()),
            (new("g4"), PieceFactory.White(PieceType.Bishop))
        );

        var rule = new IlVaticanoRule(new Offset(1, 0));
        rule.Evaluate(board, new("d4"), PieceFactory.White(PieceType.Bishop)).Should().BeEmpty();
    }
}

public class ValidIlVaticanoTestData
    : TheoryData<
        AlgebraicPoint, // origin
        AlgebraicPoint, // intermediate1
        AlgebraicPoint, // intermediate2
        AlgebraicPoint, // partnerSquare
        Offset // stepOffset
    >
{
    public ValidIlVaticanoTestData()
    {
        Add(
            new AlgebraicPoint("d4"),
            new AlgebraicPoint("e4"),
            new AlgebraicPoint("f4"),
            new AlgebraicPoint("g4"),
            new Offset(1, 0)
        );

        Add(
            new AlgebraicPoint("g4"),
            new AlgebraicPoint("f4"),
            new AlgebraicPoint("e4"),
            new AlgebraicPoint("d4"),
            new Offset(-1, 0)
        );

        Add(
            new AlgebraicPoint("d4"),
            new AlgebraicPoint("d5"),
            new AlgebraicPoint("d6"),
            new AlgebraicPoint("d7"),
            new Offset(0, 1)
        );

        Add(
            new AlgebraicPoint("d7"),
            new AlgebraicPoint("d6"),
            new AlgebraicPoint("d5"),
            new AlgebraicPoint("d4"),
            new Offset(0, -1)
        );
    }
}
