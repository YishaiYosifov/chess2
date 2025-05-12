using ErrorOr;

namespace Chess2.Api.Errors;

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

    public static Error OAuthInvalid =>
        Error.Unauthorized(ErrorCodes.AuthOAuthInvalid, "Could not authenticate via OAuth");

    public static Error OAuthLoginConflict =>
        Error.Conflict(ErrorCodes.AuthOAuthLoginConflict, "An external login already exists");

    public static Error OAuthProviderNotFound =>
        Error.NotFound(ErrorCodes.AuthOAuthProviderNotFound, "The OAuth provider was not found");
}
