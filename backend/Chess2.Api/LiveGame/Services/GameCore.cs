using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using ErrorOr;

namespace Chess2.Api.LiveGame.Services;

public interface IGameCore
{
    string InitialFen { get; }
    LegalMoveSet LegalMoves { get; }
    GameColor SideToMove { get; }

    LegalMoveSet GetLegalMovesFor(GameColor forColor);
    void InitializeGame();
    ErrorOr<MoveResult> MakeMove(MoveKey key, GameColor forColor);
}

public readonly record struct MoveResult(
    Move Move,
    MovePath MovePath,
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

    public string InitialFen { get; private set; } = "";
    public LegalMoveSet LegalMoves { get; private set; } = new();
    public GameColor SideToMove { get; private set; } = GameColor.White;

    public void InitializeGame()
    {
        var fen = _fenCalculator.CalculateFen(_board);
        InitialFen = fen;
        _drawEvaulator.RegisterInitialPosition(fen);
        CalculateAllLegalMoves(GameColor.White);
    }

    public ErrorOr<MoveResult> MakeMove(MoveKey key, GameColor forColor)
    {
        if (!LegalMoves.MovesMap.TryGetValue(key, out var move))
        {
            _logger.LogWarning("Could not find move with key {Key}", key);
            return GameErrors.MoveInvalid;
        }

        _board.PlayMove(move);
        var path = MovePath.FromMove(move, _board.Width);
        var san = _sanCalculator.CalculateSan(move, LegalMoves.AllMoves);

        CalculateAllLegalMoves(forColor.Invert());
        var fen = _fenCalculator.CalculateFen(_board);
        SideToMove = SideToMove.Invert();

        GameEndStatus? endStatus = null;
        if (_drawEvaulator.TryEvaluateDraw(move, fen, out var drawReason))
            endStatus = drawReason;

        return new MoveResult(move, path, san, endStatus);
    }

    public LegalMoveSet GetLegalMovesFor(GameColor forColor)
    {
        if (forColor != SideToMove)
            return new();
        return LegalMoves;
    }

    private void CalculateAllLegalMoves(GameColor forColor)
    {
        var allMoves = _legalMoveCalculator.CalculateAllLegalMoves(_board, forColor).ToList();
        var maxPriority =
            allMoves.Count != 0 ? allMoves.Max(m => m.ForcedPriority) : ForcedMovePriority.None;
        var legalMoves = allMoves.Where(m => m.ForcedPriority == maxPriority).ToList();

        var movePaths = legalMoves.Select(move => MovePath.FromMove(move, _board.Width)).ToList();
        var encodedMoves = _moveEncoder.EncodeMoves(movePaths);

        Dictionary<MoveKey, Move> groupedMoves = [];
        foreach (var move in legalMoves)
        {
            MoveKey key = new(From: move.From, To: move.To, PromotesTo: move.PromotesTo);
            if (!groupedMoves.TryAdd(key, move))
            {
                _logger.LogWarning(
                    "Duplicate move found from {From} to {To}. This should never happen",
                    move.From,
                    move.To
                );
                continue;
            }
        }
        LegalMoves = new(
            MovesMap: groupedMoves,
            MovePaths: movePaths,
            EncodedMoves: encodedMoves,
            HasForcedMoves: maxPriority > ForcedMovePriority.None
        );
    }
}
