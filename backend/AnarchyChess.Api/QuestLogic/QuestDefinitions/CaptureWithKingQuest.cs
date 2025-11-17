using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class CaptureWithKingQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(3, QuestDifficulty.Easy),
            CreateVariant(7, QuestDifficulty.Medium),
            CreateVariant(15, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfCaptures, QuestDifficulty difficulty) =>
        new(
            Description: $"Capture a total of {numOfCaptures} pieces WITH your king across all games",
            Difficulty: difficulty,
            Target: numOfCaptures,
            Conditions: () => [],
            Progressors: () =>
                [new OwnMoveCountMetric(new IsMoveOfPiece(PieceType.King), new IsMoveCapture())]
        );
}
