using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class NoKingMoveQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new(
                Description: "Win 2 games that lasts at least 30 moves without moving your king",
                Difficulty: QuestDifficulty.Medium,
                Target: 2,
                Conditions: () =>

                    [
                        new WinCondition(),
                        new GreaterThanEqualCondition(new MoveCountMetric(), 30),
                        new NotCondition(new PlayerPieceMovedCondition(PieceType.King)),
                    ]
            ),
        ];
}
