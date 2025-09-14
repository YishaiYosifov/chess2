using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class NoQueenMoveQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new(
                Description: "Win a game that lasts at least 30 moves without moving your queen",
                Difficulty: QuestDifficulty.Medium,
                Target: 1,
                Conditions:
                [
                    new WinCondition(),
                    new GreaterThanEqualCondition(new GameLengthMetric(), 30),
                    new NotCondition(new PlayerPieceMovedCondition(PieceType.Queen)),
                ]
            ),
        ];
}
