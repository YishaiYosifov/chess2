using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Lobby.Errors;

public class PlayerSessionErrors
{
    public static Error ConnectionAlreadySeeking =>
        Error.Conflict(
            ErrorCodes.PlayerSessionConnectionAlreadySeeking,
            "Connection already seeking in another pool"
        );

    public static Error TooManyGames =>
        Error.Forbidden(
            ErrorCodes.PlayerSessionTooManyGames,
            "Cannot start a seek because you are already at game capacity"
        );
}
