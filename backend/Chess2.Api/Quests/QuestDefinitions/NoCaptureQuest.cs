using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class NoCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            new QuestVariant(
                new WinCondition(
                    new MaxAllowedGate(new MovesBeforeFirstCaptureMetric(), maxProgress: 7)
                ),
                Description: "Win a game without a piece capture in the first 7 moves",
                Target: 1,
                Difficulty: QuestDifficulty.Easy
            ),
            new QuestVariant(
                new WinCondition(
                    new MaxAllowedGate(new MovesBeforeFirstCaptureMetric(), maxProgress: 11)
                ),
                Description: "Win a game without a piece capture in the first 11 moves",
                Target: 1,
                Difficulty: QuestDifficulty.Medium
            ),
            new QuestVariant(
                new WinCondition(
                    new MaxAllowedGate(new MovesBeforeFirstCaptureMetric(), maxProgress: 15)
                ),
                Description: "Win a game without a piece capture in the first 15 moves",
                Target: 1,
                Difficulty: QuestDifficulty.Hard
            ),
        ];
}
