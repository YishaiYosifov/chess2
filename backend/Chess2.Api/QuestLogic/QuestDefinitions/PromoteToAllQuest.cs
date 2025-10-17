using Chess2.Api.GameLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

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
