using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

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
            new WinCondition(new MaxAllowedGate(new GameLengthMetric(), maxProgress: maxMoves * 2)),
            Description: $"Win 5 games in {maxMoves} moves or less",
            Target: 5,
            Difficulty: difficulty
        );
}
