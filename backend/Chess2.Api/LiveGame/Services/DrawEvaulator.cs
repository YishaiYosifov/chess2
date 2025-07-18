using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IDrawEvaulator
{
    bool TryEvaluateDraw(Move move, string fen, [NotNullWhen(true)] out GameEndStatus? reason);
}

public class DrawEvaulator(IGameResultDescriber gameResultDescriber) : IDrawEvaulator
{
    private readonly IGameResultDescriber _gameResultDescriber = gameResultDescriber;
    private readonly Dictionary<string, int> _fenOccurrences = [];
    private int _halfMoveClock = 0;

    public bool TryEvaluateDraw(
        Move move,
        string fen,
        [NotNullWhen(true)] out GameEndStatus? endStatus
    )
    {
        if (IsThreeFold(fen))
        {
            endStatus = _gameResultDescriber.ThreeFold();
            return true;
        }
        if (Is50Moves(move))
        {
            endStatus = _gameResultDescriber.FiftyMoves();
            return true;
        }

        endStatus = null;
        return false;
    }

    private bool IsThreeFold(string fen)
    {
        if (_fenOccurrences.TryAdd(fen, 1))
            return false;

        _fenOccurrences[fen]++;
        return _fenOccurrences[fen] >= 3;
    }

    private bool Is50Moves(Move move)
    {
        if (move.Piece.Type is PieceType.Pawn || move.CapturedSquares.Any())
        {
            _halfMoveClock = 0;
            return false;
        }

        _halfMoveClock++;
        return _halfMoveClock >= 50;
    }
}
