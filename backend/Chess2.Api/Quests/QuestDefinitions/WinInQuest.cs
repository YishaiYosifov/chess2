using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class WinInQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            new QuestVariant(
                new WinCondition(new MaxAllowedGate(new GameLengthMetric(), maxProgress: 50 * 2)),
                Description: "Win in 50 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Easy
            ),
            new QuestVariant(
                new WinCondition(new MaxAllowedGate(new GameLengthMetric(), maxProgress: 25 * 2)),
                Description: "Win in 25 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Medium
            ),
            new QuestVariant(
                new WinCondition(new MaxAllowedGate(new GameLengthMetric(), maxProgress: 15 * 2)),
                Description: "Win in 15 moves or less",
                Target: 1,
                Difficulty: QuestDifficulty.Hard
            ),
        ];
}
