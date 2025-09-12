using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Metrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class CastleCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                new WinCondition(
                    new FirstOccurrenceMetric(
                        (move, _) =>
                            move.SpecialMoveType
                                is SpecialMoveType.KingsideCastle
                                    or SpecialMoveType.QueensideCastle
                            && move.Captures.Any(x => x.CapturedPiece.Type is PieceType.Bishop)
                    )
                ),
                Description: "Win a game after capturing your own bishop while castling",
                Target: 1,
                Difficulty: QuestDifficulty.Medium
            ),
        ];
}
