using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IDrawEvaulator
{
    void RegisterInitialPosition(string fen, AutoDrawState state);
    bool TryEvaluateDraw(
        Move move,
        string fen,
        AutoDrawState state,
        [NotNullWhen(true)] out GameEndStatus? reason
    );
}

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.AutoDrawState")]
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
        if (move.Piece.Type is PieceType.Pawn || move.CapturedSquares.Count != 0)
        {
            state.HalfMoveClock = 0;
            return false;
        }

        state.HalfMoveClock++;
        return state.HalfMoveClock >= 100;
    }
}
