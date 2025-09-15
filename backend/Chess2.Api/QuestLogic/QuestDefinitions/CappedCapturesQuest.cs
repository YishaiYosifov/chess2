using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class CappedCapturesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(13, QuestDifficulty.Easy),
            CreateVariant(10, QuestDifficulty.Medium),
            CreateVariant(8, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int maxCaptures, QuestDifficulty difficulty) =>
        new(
            Description: $"Win a game that lasts at least 20 moves without capturing more than {maxCaptures} pieces",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new WinCondition(),
                    new GreaterThanEqualCondition(new MoveCountMetric(), greaterThanEqual: 20),
                    new LessThanEqualCondition(
                        new OccurrenceCountMetric(
                            (move, snapshot) =>
                                move.Piece.Color == snapshot.PlayerColor && move.Captures.Count > 0
                        ),
                        lessThanEqual: maxCaptures
                    ),
                ]
        );
}
