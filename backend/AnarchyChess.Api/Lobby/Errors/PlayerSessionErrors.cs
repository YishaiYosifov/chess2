using AnarchyChess.Api.Infrastructure.Errors;
using ErrorOr;

namespace AnarchyChess.Api.Lobby.Errors;

public class PlayerSessionErrors
{
    public static Error ConnectionInGame =>
        Error.Conflict(
            ErrorCodes.PlayerSessionConnectionInGame,
            "Connection already started a game"
        );

    public static Error TooManyGames =>
        Error.Forbidden(
            ErrorCodes.PlayerSessionTooManyGames,
            "Cannot start a seek because you are already at game capacity"
        );
}
