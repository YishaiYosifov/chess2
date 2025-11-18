using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class WinStreakQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(2, QuestDifficulty.Easy),
            CreateVariant(3, QuestDifficulty.Medium),
            CreateVariant(4, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfGames, QuestDifficulty difficulty) =>
        new(
            Description: $"Win {numOfGames} games in a row without losing or drawing",
            Difficulty: difficulty,
            Target: numOfGames,
            Conditions: () => [new WinCondition()],
            ShouldResetOnFailure: true
        );
}
