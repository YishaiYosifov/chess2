using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestDefinitions;

public class NoCaptureInFirstMovesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            CreateVariant(7, QuestDifficulty.Easy),
            CreateVariant(11, QuestDifficulty.Medium),
            CreateVariant(15, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int minMoves, QuestDifficulty difficulty) =>
        new(
            Description: $"Win 3 games without a piece capture in the first {minMoves} moves (game must last at least that many moves)",
            Difficulty: difficulty,
            Target: 3,
            Conditions: () =>

                [
                    new WinCondition(),
                    new GreaterThanEqualCondition(
                        new FirstOwnMoveOccurredMetric(new IsMoveCapture()),
                        greaterThanEqual: minMoves * 2
                    ),
                    new GreaterThanEqualCondition(
                        new MoveCountMetric(),
                        greaterThanEqual: minMoves
                    ),
                ]
        );
}
