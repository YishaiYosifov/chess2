using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PerformKnooklearFusionQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(1, QuestDifficulty.Easy),
            CreateVariant(2, QuestDifficulty.Medium),
            CreateVariant(3, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfFusions, QuestDifficulty difficulty) =>
        new(
            Description: $"Perform {numOfFusions} Knooklear Fusion{(numOfFusions == 1 ? "" : "s")} in a single game",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new GreaterThanEqualCondition(
                        new OwnMoveCountMetric(new IsMoveOfType(SpecialMoveType.KnooklearFusion)),
                        numOfFusions
                    ),
                ]
        );
}
