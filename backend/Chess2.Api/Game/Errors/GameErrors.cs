using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Game.Errors;

public static class GameErrors
{
    public static Error GameNotFound =>
        Error.NotFound(ErrorCodes.GameNotFound, "Game with that token doesn't exist");

    public static Error GameAlreadyEnded =>
        Error.Forbidden(ErrorCodes.GameAlreadyEnded, "Requested game already ended");

    public static Error PlayerInvalid =>
        Error.Forbidden(
            ErrorCodes.GamePlayerInvalid,
            "The provided player is unable to perform the requested action"
        );

    public static Error MoveInvalid =>
        Error.Forbidden(ErrorCodes.GameMoveInvalid, "The provided move is illegal");
}
