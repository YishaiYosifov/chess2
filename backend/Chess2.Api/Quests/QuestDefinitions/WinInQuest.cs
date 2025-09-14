using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests.QuestDefinitions;

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
            Description: $"Win 5 games in {maxMoves} moves or less",
            Difficulty: difficulty,
            Target: 5,
            Conditions:
            [
                new WinCondition(),
                new LessThanEqualCondition(new GameLengthMetric(), lessThanEqual: maxMoves * 2),
            ]
        );
}
