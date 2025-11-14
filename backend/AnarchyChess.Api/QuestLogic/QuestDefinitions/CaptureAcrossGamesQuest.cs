using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

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
            Progressors: () => [new OwnMoveCountMetric(new IsMoveCapture())]
        );
}
