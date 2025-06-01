using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.GameLogic.Errors;

public static class GameErrors
{
    public static Error PieceNotFound =>
        Error.NotFound(ErrorCodes.GamePieceNotFound, "Piece could not be found");
}
