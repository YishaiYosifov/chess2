using ErrorOr;

namespace Chess2.Api.Errors;

public static class UserErrors
{
    public static Error UsernameTaken =>
        Error.Conflict(
            ErrorCodes.UserUsernameConflict,
            "A user with the same username already exists"
        );

    public static Error EmailTaken =>
        Error.Conflict(
            ErrorCodes.UserEmailConflict,
            "A user with the same email address already exists"
        );

    public static Error NotFound =>
        Error.NotFound(ErrorCodes.UserNotFound, "This user could not be found");

    public static Error BadCredentials =>
        Error.Unauthorized(ErrorCodes.UserBadCredentials, "Username/email/password is connect");

    public static Error SettingOnCooldown =>
        Error.Forbidden(ErrorCodes.UserSettingOnCooldown, "Cannot edit a setting, on cooldown");
}
