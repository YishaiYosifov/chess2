using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class NoCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            CreateVariant(7, QuestDifficulty.Easy),
            CreateVariant(11, QuestDifficulty.Medium),
            CreateVariant(15, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int minMoves, QuestDifficulty difficulty) =>
        new(
            Description: $"Win 5 games without a piece capture in the first {minMoves} moves (game must last at least that many moves)",
            Difficulty: difficulty,
            Target: 5,
            Conditions:
            [
                new WinCondition(),
                new GreaterThanEqualCondition(
                    new FirstOccurrenceMetric((move, _) => move.Captures.Count > 0),
                    greaterThanEqual: minMoves * 2
                ),
            ]
        );
}
