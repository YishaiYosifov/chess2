using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.LiveGame.Errors;

public static class GameErrors
{
    public static Error GameNotFound =>
        Error.NotFound(ErrorCodes.GameNotFound, "Game with that token doesn't exist");

    public static Error PlayerInvalid =>
        Error.Forbidden(
            ErrorCodes.GamePlayerInvalid,
            "The provided player is unable to perform the requested action"
        );

    public static Error MoveInvalid =>
        Error.Forbidden(ErrorCodes.GameMoveInvalid, "The provided move is illegal");

    public static Error DrawAlreadyRequested =>
        Error.Forbidden(
            ErrorCodes.GameDrawAlreadyRequested,
            "You already have a pending draw request"
        );

    public static Error DrawOnCooldown =>
        Error.Forbidden(
            ErrorCodes.GameDrawOnCooldown,
            "You cannot a draw request as you are on cooldown"
        );

    public static Error DrawNotRequested =>
        Error.Forbidden(
            ErrorCodes.GameDrawNotRequested,
            "You cannot decline a draw that doesn't exist"
        );
}
