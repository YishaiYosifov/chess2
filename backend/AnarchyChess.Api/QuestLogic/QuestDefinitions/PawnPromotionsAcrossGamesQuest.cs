using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PawnPromotionsAcrossGamesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(5, QuestDifficulty.Easy),
            CreateVariant(10, QuestDifficulty.Medium),
            CreateVariant(20, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int promotions, QuestDifficulty difficulty) =>
        new(
            Description: $"Promote a total of {promotions} pawns across all games",
            Difficulty: difficulty,
            Target: promotions,
            Conditions: () => [],
            Progressors: () =>

                [
                    new OwnMoveCountMetric(
                        new IsMoveOfPiece(PieceType.Pawn, PieceType.UnderagePawn),
                        new IsMovePromotion()
                    ),
                ]
        );
}
