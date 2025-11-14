using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class CastleCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Win 2 games after capturing your own bishop while castling",
                Difficulty: QuestDifficulty.Medium,
                Target: 2,
                Conditions: () =>

                    [
                        new WinCondition(),
                        new OwnMoveOccurredCondition(
                            new IsMoveOfType(
                                SpecialMoveType.KingsideCastle,
                                SpecialMoveType.QueensideCastle
                            ),
                            new IsMoveCaptureOf(PieceType.Bishop)
                        ),
                    ]
            ),
        ];
}
