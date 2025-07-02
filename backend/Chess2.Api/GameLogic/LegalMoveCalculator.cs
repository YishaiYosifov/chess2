using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceDefinitions;

namespace Chess2.Api.GameLogic;

public record PieceRule(
    AlgebraicPoint Offset,
    bool CanCapture = false,
    bool CaptureOnly = false,
    bool Slide = false
);

public interface ILegalMoveCalculator
{
    IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board, GameColor? forColor = null);
    IEnumerable<Move> CalculateLegalMoves(ChessBoard board, AlgebraicPoint position);
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
                throw new InvalidOperationException($"Duplicate piece definition for {piece.Type}");

            _pieceDefinitions.Add(piece.Type, piece);
        }

        if (_pieceDefinitions.Count != Enum.GetNames<PieceType>().Length)
            throw new InvalidOperationException("Could not find definitions for all pieces");
    }

    public IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board, GameColor? forColor = null)
    {
        foreach (var (position, piece) in board.EnumerateSquares())
        {
            var isColorMismatch = forColor is not null && piece?.Color != forColor;
            if (piece is null || isColorMismatch)
                continue;

            foreach (var move in CalculateLegalMoves(board, position))
                yield return move;
        }
    }

    public IEnumerable<Move> CalculateLegalMoves(ChessBoard board, AlgebraicPoint position)
    {
        if (!board.TryGetPieceAt(position, out var piece))
        {
            _logger.LogWarning("No piece found at position {Position}", position);
            yield break;
        }

        if (!_pieceDefinitions.TryGetValue(piece.Type, out var pieceDefinition))
        {
            _logger.LogWarning("Could not find definition for piece {PieceType}", piece.Type);
            yield break;
        }

        var pieceBehaviours = pieceDefinition.GetBehaviours(board, position, piece);
        foreach (var behaviour in pieceBehaviours)
        {
            foreach (var move in behaviour.Evaluate(board, position, piece))
                yield return move;
        }
    }
}
