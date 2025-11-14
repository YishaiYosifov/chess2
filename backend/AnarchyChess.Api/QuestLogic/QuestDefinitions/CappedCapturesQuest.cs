using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class CappedCapturesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(10, QuestDifficulty.Easy),
            CreateVariant(7, QuestDifficulty.Medium),
            CreateVariant(5, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int maxCaptures, QuestDifficulty difficulty) =>
        new(
            Description: $"Win a game that lasts at least 30 moves without capturing more than {maxCaptures} pieces",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new WinCondition(),
                    new GreaterThanEqualCondition(new MoveCountMetric(), greaterThanEqual: 30),
                    new LessThanEqualCondition(
                        new OwnMoveCountMetric(new IsMoveCapture()),
                        lessThanEqual: maxCaptures
                    ),
                ]
        );
}
