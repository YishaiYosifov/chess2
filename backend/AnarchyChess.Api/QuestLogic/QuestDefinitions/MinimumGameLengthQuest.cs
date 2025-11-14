using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class MinimumGameLengthQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(80, QuestDifficulty.Easy),
            CreateVariant(100, QuestDifficulty.Medium),
            CreateVariant(130, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int gameLength, QuestDifficulty difficulty) =>
        new(
            Description: $"Win a game that lasts at least {gameLength} moves",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new WinCondition(),
                    new GreaterThanEqualCondition(
                        new MoveCountMetric(),
                        greaterThanEqual: gameLength
                    ),
                ]
        );
}
