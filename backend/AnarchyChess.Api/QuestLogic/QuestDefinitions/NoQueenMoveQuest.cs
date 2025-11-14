using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

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
