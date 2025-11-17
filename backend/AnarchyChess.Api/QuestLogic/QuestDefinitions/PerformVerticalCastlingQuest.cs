using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PerformVerticalCastlingQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(1, QuestDifficulty.Easy),
            CreateVariant(2, QuestDifficulty.Medium),
            CreateVariant(3, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfGames, QuestDifficulty difficulty) =>
        new(
            Description: $"Perform vertical castling in {numOfGames} games",
            Difficulty: difficulty,
            Target: numOfGames,
            Conditions: () =>
                [new OwnMoveOccurredCondition(new IsMoveOfType(SpecialMoveType.VerticalCastle))]
        );
}
