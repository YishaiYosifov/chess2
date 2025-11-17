using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.TimeUsedMsMetric")]
public class TimeUsedMsMetric : IQuestMetric
{
    public int Evaluate(GameQuestSnapshot snapshot)
    {
        var timeControl = snapshot.FinalGameState.Pool.TimeControl;

        double timeLeft = snapshot.PlayerColor.Match(
            whenWhite: snapshot.FinalGameState.Clocks.WhiteClock,
            whenBlack: snapshot.FinalGameState.Clocks.BlackClock
        );
        int playerMoves = snapshot.PlayerColor.Match(
            whenWhite: snapshot.MoveHistory.Count / 2,
            whenBlack: (snapshot.MoveHistory.Count + 1) / 2
        );

        double timeUsedMs = timeControl.BaseSeconds * 1000 - timeLeft;
        timeUsedMs += timeControl.IncrementSeconds * 1000 * playerMoves;

        return (int)timeUsedMs;
    }
}
