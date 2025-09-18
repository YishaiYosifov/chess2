using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.QuestLogic.QuestConditions;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

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
