using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class MovesAcrossGamesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            CreateVariant(300, QuestDifficulty.Easy),
            CreateVariant(600, QuestDifficulty.Medium),
            CreateVariant(800, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfMoves, QuestDifficulty difficulty) =>
        new(
            Description: $"Make a total of {numOfMoves} moves across all games",
            Difficulty: difficulty,
            Target: numOfMoves,
            Conditions: () => [],
            Progressors: () => [new MoveCountMetric()]
        );
}
