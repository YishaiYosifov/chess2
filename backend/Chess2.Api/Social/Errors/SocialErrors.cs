using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Social.Errors;

public static class SocialErrors
{
    public static Error CannotStarSelf =>
        Error.Forbidden(ErrorCodes.SocialCannotStarSelf, "You can't star yourself");

    public static Error AlreadyStarred =>
        Error.Conflict(ErrorCodes.SocialAlreadyStarred, "User is already starred");

    public static Error NotStarred =>
        Error.NotFound(ErrorCodes.SocialNotStarred, "User is not starred");

    public static Error CannotBlockSelf =>
        Error.Forbidden(ErrorCodes.SocialCannotBlockSelf, "You can't block yourself");

    public static Error AlreadyBlocked =>
        Error.Conflict(ErrorCodes.SocialAlreadyBlocked, "User is already blocked");

    public static Error NotBlocked =>
        Error.NotFound(ErrorCodes.SocialNotBlocked, "User is not blocked");
}
