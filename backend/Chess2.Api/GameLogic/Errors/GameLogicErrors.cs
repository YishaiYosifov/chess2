﻿using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.GameLogic.Errors;

public static class GameLogicErrors
{
    public static Error PieceNotFound =>
        Error.NotFound(ErrorCodes.GameLogicPieceNotFound, "Piece could not be found");

    public static Error PointOutOfBound =>
        Error.Forbidden(
            ErrorCodes.GameLogicPointOutOfBound,
            "The point is out of the board boundaries"
        );
}
