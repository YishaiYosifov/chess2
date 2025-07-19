using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public class MoveHistoryTracker
{
    private readonly List<MoveSnapshot> _moveHistory = [];

    public IReadOnlyList<MoveSnapshot> MoveHistory => _moveHistory;
    public int MoveNumber => MoveHistory.Count;

    public MoveSnapshot RecordMove(MovePath path, string san, double timeLeft)
    {
        MoveSnapshot moveSnapshot = new(path, san, timeLeft);
        _moveHistory.Add(moveSnapshot);
        return moveSnapshot;
    }
}
