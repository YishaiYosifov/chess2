using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Matchmaking.Errors;

public class MatchmakingErrors
{
    public static Error MatchNotFound =>
        Error.NotFound(ErrorCodes.MatchmakingMatchNotFound, "No match could be found");
}
