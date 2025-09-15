using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class WinInQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            CreateVariant(35, QuestDifficulty.Easy),
            CreateVariant(25, QuestDifficulty.Medium),
            CreateVariant(15, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int maxMoves, QuestDifficulty difficulty) =>
        new(
            Description: $"Win 3 games in {maxMoves} moves or less",
            Difficulty: difficulty,
            Target: 3,
            Conditions: () =>

                [
                    new WinCondition(),
                    new LessThanEqualCondition(new MoveCountMetric(), lessThanEqual: maxMoves),
                ]
        );
}
