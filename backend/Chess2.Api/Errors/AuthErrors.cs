using ErrorOr;

namespace Chess2.Api.Errors;

public static class AuthErrors
{
    public static Error TokenMissing =>
        Error.Unauthorized("Auth.TokenMissing", "The required authentication token is missing");

    public static Error TokenInvalid =>
        Error.Unauthorized("Auth.TokenInvalid", "The authentication token provided is invalid");

    public static Error IncorrectUserType =>
        Error.Unauthorized("Auth.WrongType", "Authed user type is incorrect");
}
