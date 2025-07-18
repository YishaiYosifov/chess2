using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public class MoveHistoryTracker
{
    private readonly List<MoveSnapshot> _moveHistory = [];

    public IReadOnlyList<MoveSnapshot> MoveHistory => _moveHistory;
    public int MoveNumber => MoveHistory.Count;

    public MoveSnapshot RecordMove(string encoded, string san, double timeLeft)
    {
        MoveSnapshot moveSnapshot = new(encoded, san, timeLeft);
        _moveHistory.Add(moveSnapshot);
        return moveSnapshot;
    }
}
