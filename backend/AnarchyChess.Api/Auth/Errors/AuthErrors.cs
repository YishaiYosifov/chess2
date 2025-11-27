using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Auth.Errors;

public static class AuthErrors
{
    public static Error TokenMissing =>
        Error.Unauthorized(
            ErrorCodes.AuthTokenMissing,
            "The required authentication token is missing"
        );

    public static Error TokenInvalid =>
        Error.Unauthorized(
            ErrorCodes.AuthTokenInvalid,
            "The authentication token provided is invalid"
        );

    public static Error UserBanned =>
        Error.Forbidden(
            ErrorCodes.AuthUserBanned,
            "You cannot perform this action as your account is banned"
        );

    public static Error OAuthInvalid =>
        Error.Unauthorized(ErrorCodes.AuthOAuthInvalid, "Could not authenticate via OAuth");

    public static Error OAuthProviderNotFound =>
        Error.NotFound(ErrorCodes.AuthOAuthProviderNotFound, "The OAuth provider was not found");
}
