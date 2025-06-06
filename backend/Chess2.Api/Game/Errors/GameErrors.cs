using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Game.Errors;

public static class GameErrors
{
    public static Error GameNotFound =>
        Error.NotFound(ErrorCodes.GameNotFound, "Game with that token doesn't exist");
}
