using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PromoteToAllQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Promote to every piece types at least once across all games",
                Difficulty: QuestDifficulty.Medium,
                Target: GameLogicConstants.PromotablePieces.Count,
                Conditions: () => [],
                Progressors: () => [new ProgressiveUniquePromotionsMetric()]
            ),
        ];
}
