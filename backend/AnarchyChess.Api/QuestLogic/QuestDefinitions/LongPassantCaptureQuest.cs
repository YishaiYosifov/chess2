using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class LongPassantCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Perform a long passant that captures 2+ pieces",
                Difficulty: QuestDifficulty.Easy,
                Target: 1,
                Conditions: () =>

                    [
                        new OwnMoveOccurredCondition(
                            new IsMoveOfType(SpecialMoveType.EnPassant),
                            new IsMoveCapture(ofAtLeast: 2)
                        ),
                    ]
            ),
        ];
}
