using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Game.Services;

public interface IDrawEvaulator
{
    void RegisterInitialPosition(string fen, AutoDrawState state);
    bool TryEvaluateDraw(
        Move move,
        string fen,
        IReadOnlyChessBoard board,
        AutoDrawState state,
        [NotNullWhen(true)] out GameEndStatus? endStatus
    );
}

[GenerateSerializer]
[Alias("Chess2.Api.Game.Services.AutoDrawState")]
public class AutoDrawState
{
    [Id(0)]
    public Dictionary<string, int> FenOccurrences { get; init; } = [];

    [Id(1)]
    public int HalfMoveClock { get; set; }
}

public class DrawEvaulator(IGameResultDescriber gameResultDescriber) : IDrawEvaulator
{
    private readonly IGameResultDescriber _gameResultDescriber = gameResultDescriber;

    public void RegisterInitialPosition(string fen, AutoDrawState state) =>
        state.FenOccurrences.TryAdd(fen, 1);

    public bool TryEvaluateDraw(
        Move move,
        string fen,
        IReadOnlyChessBoard board,
        AutoDrawState state,
        [NotNullWhen(true)] out GameEndStatus? endStatus
    )
    {
        if (IsThreeFold(fen, state))
        {
            endStatus = _gameResultDescriber.ThreeFold();
            return true;
        }
        if (Is50Moves(move, state))
        {
            endStatus = _gameResultDescriber.FiftyMoves();
            return true;
        }
        if (IsKingTouch(move, board))
        {
            endStatus = _gameResultDescriber.KingTouch();
            return true;
        }

        endStatus = null;
        return false;
    }

    private static bool IsThreeFold(string fen, AutoDrawState state)
    {
        if (state.FenOccurrences.TryAdd(fen, 1))
            return false;

        state.FenOccurrences[fen]++;
        return state.FenOccurrences[fen] >= 3;
    }

    private static bool Is50Moves(Move move, AutoDrawState state)
    {
        if (GameLogicConstants.PawnLikePieces.Contains(move.Piece.Type) || move.Captures.Count != 0)
        {
            state.HalfMoveClock = 0;
            return false;
        }

        state.HalfMoveClock++;
        return state.HalfMoveClock >= 100;
    }

    private static bool IsKingTouch(Move move, IReadOnlyChessBoard board)
    {
        if (move.Piece.Type is not PieceType.King)
            return false;

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (y == 0 && x == 0)
                    continue;

                var position = move.To - new Offset(x, y);
                if (
                    board.TryGetPieceAt(position, out var touchingPiece)
                    && touchingPiece.Type is PieceType.King
                    && touchingPiece.Color != move.Piece.Color
                )
                    return true;
            }
        }

        return false;
    }
}
