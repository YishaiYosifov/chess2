using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PerformIlVaticano : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(1, QuestDifficulty.Easy),
            CreateVariant(3, QuestDifficulty.Medium),
            CreateVariant(5, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfGames, QuestDifficulty difficulty) =>
        new(
            Description: $"Perform il vaticano in {numOfGames} separate games",
            Difficulty: difficulty,
            Target: numOfGames,
            Conditions: () =>
                [new OwnMoveOccurredCondition(new IsMoveOfType(SpecialMoveType.IlVaticano))]
        );
}
