using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

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
            new WinCondition(
                new MinAllowedGate(
                    new FirstOccurrenceMetric((move, _) => move.Captures.Count > 0),
                    minProgress: minMoves * 2
                )
            ),
            Description: $"Win 5 games without a piece capture in the first {minMoves} moves",
            Target: 5,
            Difficulty: difficulty
        );
}
