using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.GameLogic.Errors;

public static class GameLogicErrors
{
    public static Error PieceNotFound =>
        Error.NotFound(ErrorCodes.GameLogicPieceNotFound, "Piece could not be found");
}
