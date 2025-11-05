using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Tournaments.Errors;

public static class TournamentErrors
{
    public static Error TournamentAlreadyExists =>
        Error.Conflict(
            ErrorCodes.TournamentAlreadyExists,
            "A tournament has already been created for this ID"
        );

    public static Error TournamentNotFound =>
        Error.NotFound(ErrorCodes.TournamentNotFound, "Tournament not found for the given ID");

    public static Error NoHostPermissions =>
        Error.Forbidden(
            ErrorCodes.TournamentNoHostPermissions,
            "You must be the host to perform this action"
        );

    public static Error CannotEnterTournament =>
        Error.Forbidden(ErrorCodes.TournamentCannotEnter, "You cannot enter this tournament");

    public static Error NotPartOfTournament =>
        Error.Conflict(
            ErrorCodes.TournamentNotPartOf,
            "You cannot perform this action because you are not part of this tournament"
        );
}
