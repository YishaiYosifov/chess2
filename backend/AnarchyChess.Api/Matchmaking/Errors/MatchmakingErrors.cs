using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Matchmaking.Errors;

public static class MatchmakingErrors
{
    public static Error SeekNotFound =>
        Error.NotFound(ErrorCodes.MatchmakingSeekNotFound, "Could not find the requested seek");

    public static Error RequestedSeekerNotCompatible =>
        Error.Forbidden(
            ErrorCodes.MatchmakingSeekerNotCompatible,
            "The requested seeker could not be matched with you"
        );
}
