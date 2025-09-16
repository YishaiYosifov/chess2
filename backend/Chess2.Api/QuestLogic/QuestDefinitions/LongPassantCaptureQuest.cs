using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

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
                            (move, snapshot) =>
                                move.SpecialMoveType is SpecialMoveType.EnPassant
                                && move.Captures.Count >= 2
                        ),
                    ]
            ),
        ];
}
