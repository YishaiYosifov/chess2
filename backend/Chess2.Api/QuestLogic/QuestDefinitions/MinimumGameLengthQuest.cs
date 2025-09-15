using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

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
                        new GameLengthMetric(),
                        greaterThanEqual: gameLength * 2
                    ),
                ]
        );
}
