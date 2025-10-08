using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Challenges.Errors;

public static class ChallengeErrors
{
    public static Error RecipientNotAccepting =>
        Error.Forbidden(
            ErrorCodes.ChallengeRecipientNotAccepting,
            "The recipient is not accepting challenges from you"
        );

    public static Error CannotChallengeSelf =>
        Error.Validation(ErrorCodes.ChallengeCannotChallengeSelf, "You cannot challenge yourself");

    public static Error AlreadyExists =>
        Error.Conflict(
            ErrorCodes.ChallengeAlreadyExists,
            "You already sent a challenge to this user"
        );

    public static Error AuthedOnlyPool =>
        Error.Forbidden(
            ErrorCodes.ChallengeAuthedOnlyPool,
            "Only a logged in user can interact with this pool type"
        );

    public static Error NotFound =>
        Error.NotFound(ErrorCodes.ChallengeNotFound, "Challenge not found");
}
