using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceDefinitions;

namespace Chess2.Api.GameLogic;

public record PieceRule(
    Point Offset,
    bool CanCapture = false,
    bool CaptureOnly = false,
    bool Slide = false
);

public interface ILegalMoveCalculator
{
    IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board);
}

public class LegalMoveCalculator : ILegalMoveCalculator
{
    private readonly Dictionary<PieceType, IPieceDefinition> _pieceDefinitions = [];
    private readonly ILogger<LegalMoveCalculator> _logger;

    public LegalMoveCalculator(
        ILogger<LegalMoveCalculator> logger,
        IEnumerable<IPieceDefinition> pieceDefinitions
    )
    {
        _logger = logger;

        foreach (var piece in pieceDefinitions)
        {
            if (_pieceDefinitions.ContainsKey(piece.Type))
                throw new InvalidOperationException(
                    $"Duplicate piece definitions for {piece.Type}"
                );

            _pieceDefinitions.Add(piece.Type, piece);
        }

        if (_pieceDefinitions.Count != Enum.GetNames<PieceType>().Length)
            throw new InvalidOperationException("Could not find definitions for all pieces");
    }

    public IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board)
    {
        foreach (var (position, piece) in board.GetSquares())
        {
            if (piece is null)
                continue;

            if (!_pieceDefinitions.TryGetValue(piece.Type, out var pieceDefinition))
            {
                _logger.LogWarning("Could not find definition for piece {PieceType}", piece.Type);
                continue;
            }

            var pieceBehaviours = pieceDefinition.GetBehaviours(board, position, piece);
            foreach (var behaviour in pieceBehaviours)
            {
                foreach (var move in behaviour.Evaluate(board, position, piece))
                    yield return move;
            }
        }
    }
}
