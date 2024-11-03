namespace Chess2.Api.Errors;

public static class UserErrors
{
    public static Error UsernameTaken =>
        new ConflictError("User.UsernameTaken", "A user with the same username already exists");

    public static Error EmailTaken =>
        new ConflictError("User.EmailTaken", "A user with the same email address already exists");
}
