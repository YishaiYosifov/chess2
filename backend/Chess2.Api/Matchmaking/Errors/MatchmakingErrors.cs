using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Matchmaking.Errors;

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
