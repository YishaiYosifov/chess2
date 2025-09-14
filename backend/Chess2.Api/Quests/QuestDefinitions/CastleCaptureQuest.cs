using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class CastleCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                new WinCondition(
                    new MinAllowedGate(
                        new OccurrenceCountMetric(
                            (move, _) =>
                                move.SpecialMoveType
                                    is SpecialMoveType.KingsideCastle
                                        or SpecialMoveType.QueensideCastle
                                && move.Captures.Any(x => x.CapturedPiece.Type is PieceType.Bishop)
                        ),
                        1
                    )
                ),
                Description: "Win 2 games after capturing your own bishop while castling",
                Target: 2,
                Difficulty: QuestDifficulty.Medium
            ),
        ];
}
