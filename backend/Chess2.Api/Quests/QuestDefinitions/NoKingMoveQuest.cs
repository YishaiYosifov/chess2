using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.Quests.QuestProgressors.Metrics;

namespace Chess2.Api.Quests.QuestDefinitions;

public class NoKingMoveQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new(
                Progressor: new WinCondition(
                    new AndGate(
                        new GreaterThanEqualGate(new GameLengthMetric(), 30),
                        new LessThanEqualGate(
                            new OccurrenceCountMetric(
                                (move, snapshot) =>
                                    move.Piece.Color == snapshot.PlayerColor
                                    && move.Piece.Type is PieceType.King
                            ),
                            lessThanEqual: 0
                        )
                    )
                ),
                Description: "Win a game that lasts at least 30 moves without moving your king",
                Target: 1,
                Difficulty: QuestDifficulty.Medium
            ),
        ];
}
