using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.MoveConditions.IsMoveOfPiece")]
public class IsMoveOfPiece(PieceType piece) : IMoveCondition
{
    [Id(0)]
    private readonly PieceType _piece = piece;

    public bool Evaluate(Move move) => move.Piece.Type == _piece;
}
