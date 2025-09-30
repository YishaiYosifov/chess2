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

    public static Error CannotAccept =>
        Error.Forbidden(ErrorCodes.ChallengeCannotAccept, "You cannot accept this challenge");
}
