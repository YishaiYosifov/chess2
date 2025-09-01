using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Social.Errors;

public static class SocialErrors
{
    public static Error AlreadyStarred =>
        Error.Conflict(ErrorCodes.SocialAlreadyStarred, "User is already starred");

    public static Error NotStarred =>
        Error.NotFound(ErrorCodes.SocialNotStarred, "User is not starred");
}
