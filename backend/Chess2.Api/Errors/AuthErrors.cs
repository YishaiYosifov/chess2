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
}
