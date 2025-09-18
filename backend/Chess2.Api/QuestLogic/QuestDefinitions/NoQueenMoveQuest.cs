using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class NoQueenMoveQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new(
                Description: "Win 2 games that last at least 30 moves without moving your queen",
                Difficulty: QuestDifficulty.Medium,
                Target: 2,
                Conditions: () =>

                    [
                        new WinCondition(),
                        new GreaterThanEqualCondition(new MoveCountMetric(), 30),
                        new NotCondition(
                            new OwnMoveOccurredCondition(new IsMoveOfPiece(PieceType.Queen))
                        ),
                    ]
            ),
        ];
}
