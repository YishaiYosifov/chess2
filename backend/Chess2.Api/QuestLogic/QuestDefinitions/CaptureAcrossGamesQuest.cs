using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class CaptureAcrossGamesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(20, QuestDifficulty.Easy),
            CreateVariant(50, QuestDifficulty.Medium),
            CreateVariant(80, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int captures, QuestDifficulty difficulty) =>
        new(
            Description: $"Capture a total of {captures} pieces across multiple games",
            Difficulty: difficulty,
            Target: captures,
            Conditions: () => [],
            Progressors: () => [new OwnMoveCountMetric((move, snapshot) => move.Captures.Count > 0)]
        );
}
