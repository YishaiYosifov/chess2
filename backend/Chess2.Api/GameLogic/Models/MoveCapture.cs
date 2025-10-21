namespace Chess2.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.Models.MoveCapture")]
public record MoveCapture(Piece CapturedPiece, AlgebraicPoint Position)
{
    public MoveCapture(AlgebraicPoint position, IReadOnlyChessBoard board)
        : this(
            board.PeekPieceAt(position)
                ?? throw new InvalidOperationException($"No piece at {position}"),
            position
        ) { }
}
