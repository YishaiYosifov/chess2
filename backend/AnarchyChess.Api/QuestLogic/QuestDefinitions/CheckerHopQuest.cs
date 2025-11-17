using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class CheckerHopQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [CreateVariant(2, QuestDifficulty.Easy), CreateVariant(3, QuestDifficulty.Medium)];

    private static QuestVariant CreateVariant(int piecesToCapture, QuestDifficulty difficulty) =>
        new(
            Description: $"Perform a checker multi-hop that captures at least {piecesToCapture} pieces",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new OwnMoveOccurredCondition(
                        new IsMoveOfPiece(PieceType.Checker),
                        new IsMoveCapture(ofAtLeast: piecesToCapture)
                    ),
                ]
        );
}
