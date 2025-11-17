using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class WinGameWith2Kings : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Win a game with 2 kings of your own color on the board (promote your checker to a king)",
                Difficulty: QuestDifficulty.Medium,
                Target: 1,
                Conditions: () =>

                    [
                        new WinCondition(),
                        new OwnMoveOccurredCondition(
                            new IsMovePromotion(promotesTo: PieceType.King)
                        ),
                        new NotCondition(
                            new OpponentMoveOccurredCondition(new IsMoveCaptureOf(PieceType.King))
                        ),
                    ]
            ),
        ];
}
