using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestConditions.PlayerPieceMovedCondition")]
public class PlayerPieceMovedCondition(PieceType pieceType) : IQuestCondition
{
    [Id(0)]
    private readonly PieceType pieceType = pieceType;

    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.MoveHistory.Any(move =>
            move.Piece.Color == snapshot.PlayerColor && move.Piece.Type == pieceType
        );
}
