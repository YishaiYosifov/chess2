using Chess2.Api.Game.Errors;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
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

namespace Chess2.Api.Game.Services;

public interface IGameCore
{
    string Fen { get; }
    LegalMoveSet LegalMoves { get; }
    GameColor SideToMove { get; }

    LegalMoveSet GetLegalMovesFor(GameColor forColor);
    void InitializeGame();
    ErrorOr<(Move move, string EncodedMove, string San)> MakeMove(
        AlgebraicPoint from,
        AlgebraicPoint to,
        GameColor forColor
    );
}

public record LegalMoveSet(IReadonlyLegalMoveMap Moves, IEnumerable<string> EncodedMoves)
{
    public IEnumerable<Move> AllMoves => Moves.Values;

    public LegalMoveSet()
        : this(new LegalMoveMap(), []) { }
}

public class GameCore(
    ILogger<GameCore> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder,
    ISanCalculator sanCalculator
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

    public string Fen { get; private set; } = "";
    public LegalMoveSet LegalMoves { get; private set; } = new();
    public GameColor SideToMove { get; private set; } = GameColor.White;

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        CalculateAllLegalMoves(GameColor.White);
    }

    public ErrorOr<(Move move, string EncodedMove, string San)> MakeMove(
        AlgebraicPoint from,
        AlgebraicPoint to,
        GameColor forColor
    )
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

        return (move, encodedMove, san);
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
