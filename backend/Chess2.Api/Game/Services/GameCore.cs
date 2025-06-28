using Chess2.Api.Game.Errors;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameCore
{
    string Fen { get; }
    IReadOnlyCollection<string> EncodedMoveHistory { get; }
    int MoveNumber { get; }
    GameColor SideToMove { get; }

    void InitializeGame();
    IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor);
    ErrorOr<string> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor);
    IReadOnlyDictionary<(AlgebraicPoint from, AlgebraicPoint to), Move> GetLegalMovesFor(
        GameColor forColor
    );
}

public record LegalMoveSet(
    IReadOnlyDictionary<(AlgebraicPoint from, AlgebraicPoint to), Move> Moves,
    IReadOnlyList<string> EncodedMoves
);

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

    private Dictionary<GameColor, LegalMoveSet> _legalMovesByColor = [];

    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;

    public string Fen { get; private set; } = "";
    public IReadOnlyCollection<string> EncodedMoveHistory => _encodedMoveHistory.AsReadOnly();
    public int MoveNumber => EncodedMoveHistory.Count;
    public GameColor SideToMove => MoveNumber % 2 == 0 ? GameColor.White : GameColor.Black;

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        CalculateAllLegalMoves();
    }

    public IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor) =>
        _legalMovesByColor.TryGetValue(forColor, out var set) ? set.EncodedMoves : [];

    public IReadOnlyDictionary<(AlgebraicPoint from, AlgebraicPoint to), Move> GetLegalMovesFor(
        GameColor forColor
    ) =>
        _legalMovesByColor.TryGetValue(forColor, out var set)
            ? set.Moves
            : new Dictionary<(AlgebraicPoint, AlgebraicPoint), Move>();

    public ErrorOr<string> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor)
    {
        var legalMoves = GetLegalMovesFor(forColor);
        if (!legalMoves.TryGetValue((from, to), out var move))
        {
            _logger.LogWarning("Could not find move from {From} to {To}", from, to);
            return GameErrors.MoveInvalid;
        }

        _board.PlayMove(move);

        CalculateAllLegalMoves();
        Fen = _fenCalculator.CalculateFen(_board);

        var encodedMove = _moveEncoder.EncodeSingleMove(move);
        _encodedMoveHistory.Add(encodedMove);
        return encodedMove;
    }

    private void CalculateAllLegalMoves()
    {
        var allMoves = _legalMoveCalculator.CalculateAllLegalMoves(_board);

        _legalMovesByColor = [];
        foreach (var group in allMoves.GroupBy(m => m.Piece.Color))
        {
            var colorMoves = new Dictionary<(AlgebraicPoint from, AlgebraicPoint to), Move>();
            foreach (var move in group)
            {
                var key = (move.From, move.To);
                if (!colorMoves.TryAdd(key, move))
                {
                    _logger.LogWarning(
                        "Duplicate move found from {From} to {To}",
                        move.From,
                        move.To
                    );
                    continue;
                }
            }
            var encodedMoves = _moveEncoder.EncodeMoves(colorMoves.Values).ToList();

            _legalMovesByColor[group.Key] = new LegalMoveSet(colorMoves, encodedMoves);
        }
    }
}
