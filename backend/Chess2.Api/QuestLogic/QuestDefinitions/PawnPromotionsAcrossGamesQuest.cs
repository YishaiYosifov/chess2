using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class PawnPromotionsAcrossGamesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(5, QuestDifficulty.Easy),
            CreateVariant(10, QuestDifficulty.Medium),
            CreateVariant(20, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int promotions, QuestDifficulty difficulty) =>
        new(
            Description: $"Promote {promotions} pawns across all games",
            Difficulty: difficulty,
            Target: promotions,
            Conditions: () => [],
            Progressors: () =>
                [new OwnMoveCountMetric((move, snapshot) => move.PromotesTo is not null)]
        );
}
