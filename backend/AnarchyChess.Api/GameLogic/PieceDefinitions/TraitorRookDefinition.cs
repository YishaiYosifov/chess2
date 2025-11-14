using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class TraitorRookDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.TraitorRook;

    private static readonly IMovementBehaviour[] _rookMoves =
    [
        new SlideBehaviour(new Offset(X: 0, Y: 1)),
        new SlideBehaviour(new Offset(X: 0, Y: -1)),
        new SlideBehaviour(new Offset(X: 1, Y: 0)),
        new SlideBehaviour(new Offset(X: -1, Y: 0)),
    ];

    private readonly IPieceMovementRule _blackMajority = CaptureRule.WithNeutralCapture(
        allowCaptureWhen: (board, piece) => piece.Color != GameColor.Black,
        _rookMoves
    );

    private readonly IPieceMovementRule _whiteMajority = CaptureRule.WithNeutralCapture(
        allowCaptureWhen: (board, piece) => piece.Color != GameColor.White,
        _rookMoves
    );

    private readonly IPieceMovementRule _neutral = new NoCaptureRule(_rookMoves);

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    )
    {
        int whitePieces = 0;
        int blackPieces = 0;
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                AlgebraicPoint targetPosition = position + new Offset(x, y);
                if (!board.TryGetPieceAt(targetPosition, out var targetPiece))
                    continue;

                if (targetPiece.Color is GameColor.White)
                    whitePieces++;
                else if (targetPiece.Color is GameColor.Black)
                    blackPieces++;
            }
        }

        if (whitePieces > blackPieces && movingPlayer is GameColor.White)
            yield return _whiteMajority;
        else if (blackPieces > whitePieces && movingPlayer is GameColor.Black)
            yield return _blackMajority;
        else if (blackPieces > 0 && whitePieces > 0 && blackPieces == whitePieces)
            yield return _neutral;
    }
}
