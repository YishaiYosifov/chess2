using ErrorOr;

namespace Chess2.Api.Errors;

public static class UserErrors
{
    public static Error UsernameTaken =>
        Error.Conflict("User.UsernameTaken", "A user with the same username already exists");

    public static Error EmailTaken =>
        Error.Conflict("User.EmailTaken", "A user with the same email address already exists");
}
