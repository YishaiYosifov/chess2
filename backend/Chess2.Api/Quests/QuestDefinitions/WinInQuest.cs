using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors;

namespace Chess2.Api.Quests.QuestDefinitions;

public class WinInQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            new QuestVariant(
                new WinCondition(
                    new MaxProgressGate(new GameLengthProgress(), maxProgress: 50 * 2)
                ),
                Description: "Win in 50 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Easy
            ),
            new QuestVariant(
                new WinCondition(
                    new MaxProgressGate(new GameLengthProgress(), maxProgress: 25 * 2)
                ),
                Description: "Win in 25 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Medium
            ),
            new QuestVariant(
                new WinCondition(
                    new MaxProgressGate(new GameLengthProgress(), maxProgress: 15 * 2)
                ),
                Description: "Win in 15 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Hard
            ),
        ];
}
