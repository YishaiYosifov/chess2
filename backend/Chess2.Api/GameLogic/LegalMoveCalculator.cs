using Chess2.Api.GameLogic.ForeverRules;
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
    IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board, GameColor movingPlayer);
    IEnumerable<Move> CalculateLegalMoves(
        ChessBoard board,
        AlgebraicPoint position,
        GameColor movingPlayer
    );
}

public class LegalMoveCalculator : ILegalMoveCalculator
{
    private readonly Dictionary<PieceType, IPieceDefinition> _pieceDefinitions = [];
    private readonly IEnumerable<IForeveRule> _foreverRules;

    public LegalMoveCalculator(
        IEnumerable<IPieceDefinition> pieceDefinitions,
        IEnumerable<IForeveRule> foreverRules
    )
    {
        _pieceDefinitions = pieceDefinitions.ToDictionary(definition => definition.Type);
        if (_pieceDefinitions.Count != Enum.GetNames<PieceType>().Length)
            throw new InvalidOperationException("Could not find definitions for all pieces");

        _foreverRules = foreverRules;
    }

    public IEnumerable<Move> CalculateAllLegalMoves(ChessBoard board, GameColor movingPlayer)
    {
        foreach (var (position, piece) in board.EnumeratePieces())
        {
            foreach (var move in CalculateLegalMoves(board, position, movingPlayer))
                yield return move;
        }

        foreach (var rule in _foreverRules)
        {
            foreach (var move in rule.GetBehaviours(board, movingPlayer))
                yield return move;
        }
    }

    public IEnumerable<Move> CalculateLegalMoves(
        ChessBoard board,
        AlgebraicPoint position,
        GameColor movingPlayer
    )
    {
        if (!board.TryGetPieceAt(position, out var piece))
            yield break;

        var isColorMismatch = piece.Color is not null && piece.Color != movingPlayer;
        if (isColorMismatch)
            yield break;

        var pieceBehaviours = _pieceDefinitions[piece.Type]
            .GetBehaviours(board, position, piece, movingPlayer);
        foreach (var behaviour in pieceBehaviours)
        {
            foreach (var move in behaviour.Evaluate(board, position, piece))
                yield return move;
        }
    }
}
