using Chess2.Api.Game.Errors;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameCore
{
    string Fen { get; }
    IEnumerable<string> EncodedMoveHistory { get; }
    int MoveNumber { get; }
    GameColor SideToMove { get; }

    LegalMoveSet GetLegalMovesFor(GameColor forColor);
    void InitializeGame();
    ErrorOr<string> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor);
}

public record LegalMoveSet(
    IReadOnlyDictionary<(AlgebraicPoint from, AlgebraicPoint to), Move> Moves,
    IEnumerable<string> EncodedMoves
)
{
    public IEnumerable<Move> AllMoves => Moves.Values;

    public LegalMoveSet()
        : this(new Dictionary<(AlgebraicPoint from, AlgebraicPoint to), Move>(), []) { }
}

public class GameCore(
    ILogger<GameCore> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder
) : IGameCore
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
    private readonly List<string> _encodedMoveHistory = [];

    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;

    public string Fen { get; private set; } = "";
    public IEnumerable<string> EncodedMoveHistory => _encodedMoveHistory.AsReadOnly();
    public int MoveNumber => EncodedMoveHistory.Count();
    public GameColor SideToMove => MoveNumber % 2 == 0 ? GameColor.White : GameColor.Black;
    public LegalMoveSet LegalMoves { get; private set; } = new();

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        CalculateAllLegalMoves(GameColor.White);
    }

    public ErrorOr<string> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor)
    {
        if (!LegalMoves.Moves.TryGetValue((from, to), out var move))
        {
            _logger.LogWarning("Could not find move from {From} to {To}", from, to);
            return GameErrors.MoveInvalid;
        }

        _board.PlayMove(move);

        CalculateAllLegalMoves(forColor.Invert());
        Fen = _fenCalculator.CalculateFen(_board);

        var encodedMove = _moveEncoder.EncodeSingleMove(move);
        _encodedMoveHistory.Add(encodedMove);
        return encodedMove;
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

        var groupedLegalMoves = new Dictionary<(AlgebraicPoint from, AlgebraicPoint to), Move>();
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
