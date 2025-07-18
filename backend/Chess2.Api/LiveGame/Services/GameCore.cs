using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Errors;
using ErrorOr;
using IReadonlyLegalMoveMap = System.Collections.Generic.IReadOnlyDictionary<
    (
        Chess2.Api.GameLogic.Models.AlgebraicPoint from,
        Chess2.Api.GameLogic.Models.AlgebraicPoint to
    ),
    Chess2.Api.GameLogic.Models.Move
>;
using LegalMoveMap = System.Collections.Generic.Dictionary<
    (
        Chess2.Api.GameLogic.Models.AlgebraicPoint from,
        Chess2.Api.GameLogic.Models.AlgebraicPoint to
    ),
    Chess2.Api.GameLogic.Models.Move
>;

namespace Chess2.Api.LiveGame.Services;

public interface IGameCore
{
    string Fen { get; }
    LegalMoveSet LegalMoves { get; }
    GameColor SideToMove { get; }

    LegalMoveSet GetLegalMovesFor(GameColor forColor);
    void InitializeGame();
    ErrorOr<MoveResult> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor);
}

public record LegalMoveSet(IReadonlyLegalMoveMap Moves, IEnumerable<string> EncodedMoves)
{
    public IEnumerable<Move> AllMoves => Moves.Values;

    public LegalMoveSet()
        : this(new LegalMoveMap(), []) { }
}

public readonly record struct MoveResult(
    Move Move,
    string EncodedMove,
    string San,
    GameEndStatus? EndStatus
);

public class GameCore(
    ILogger<GameCore> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder,
    ISanCalculator sanCalculator,
    IDrawEvaulator drawEvaulator
) : IGameCore
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );

    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;
    private readonly ISanCalculator _sanCalculator = sanCalculator;
    private readonly IDrawEvaulator _drawEvaulator = drawEvaulator;

    public string Fen { get; private set; } = "";
    public LegalMoveSet LegalMoves { get; private set; } = new();
    public GameColor SideToMove { get; private set; } = GameColor.White;

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        CalculateAllLegalMoves(GameColor.White);
    }

    public ErrorOr<MoveResult> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor)
    {
        if (!LegalMoves.Moves.TryGetValue((from, to), out var move))
        {
            _logger.LogWarning("Could not find move from {From} to {To}", from, to);
            return GameErrors.MoveInvalid;
        }

        _board.PlayMove(move);
        var encodedMove = _moveEncoder.EncodeSingleMove(move);
        var san = _sanCalculator.CalculateSan(move, LegalMoves.AllMoves);

        CalculateAllLegalMoves(forColor.Invert());
        Fen = _fenCalculator.CalculateFen(_board);
        SideToMove = SideToMove.Invert();

        GameEndStatus? endStatus = null;
        if (_drawEvaulator.TryEvaluateDraw(move, Fen, out var drawReason))
            endStatus = drawReason;

        return new MoveResult(move, encodedMove, san, endStatus);
    }

    public LegalMoveSet GetLegalMovesFor(GameColor forColor)
    {
        if (forColor != SideToMove)
            return new();
        return LegalMoves;
    }

    private void CalculateAllLegalMoves(GameColor forColor)
    {
        var legalMoves = _legalMoveCalculator.CalculateAllLegalMoves(_board, forColor).ToList();
        var encodedLegalMoves = _moveEncoder.EncodeMoves(legalMoves).ToList();

        LegalMoveMap groupedLegalMoves = [];
        foreach (var move in legalMoves)
        {
            var key = (move.From, move.To);
            if (!groupedLegalMoves.TryAdd(key, move))
            {
                _logger.LogWarning(
                    "Duplicate move found from {From} to {To}. This should never happen",
                    move.From,
                    move.To
                );
                continue;
            }
        }
        LegalMoves = new(Moves: groupedLegalMoves, EncodedMoves: encodedLegalMoves);
    }
}
