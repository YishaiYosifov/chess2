using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.ProgressiveUniquePromotionsMetric")]
public class ProgressiveUniquePromotionsMetric : IQuestMetric
{
    [Id(0)]
    private readonly HashSet<PieceType> _promotedTo = [];

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int newPromotions = 0;
        foreach (var move in snapshot.MoveHistory)
        {
            if (move.Piece.Color != snapshot.PlayerColor || !move.PromotesTo.HasValue)
                continue;

            var promotesTo = move.PromotesTo.Value;
            if (_promotedTo.Contains(promotesTo))
                continue;

            _promotedTo.Add(promotesTo);
            newPromotions++;
        }

        return newPromotions;
    }
}
