namespace Chess2.Api.GameLogic.Models;

public record MoveCapture(Piece CapturedPiece, AlgebraicPoint Position)
{
    public MoveCapture(AlgebraicPoint position, ChessBoard board)
        : this(
            board.PeekPieceAt(position)
                ?? throw new InvalidOperationException($"No piece at {position}"),
            position
        ) { }
}
