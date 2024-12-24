using ErrorOr;

namespace Chess2.Api.Errors;

public static class UserErrors
{
    public static Error UsernameTaken =>
        Error.Conflict(
            "User.Conflict",
            "A user with the same username already exists",
            new() { { MetadataFields.RelatedField, "username" } }
        );

    public static Error EmailTaken =>
        Error.Conflict(
            "User.Conflict",
            "A user with the same email address already exists",
            new() { { MetadataFields.RelatedField, "email" } }
        );

    public static Error UserNotFound =>
        Error.NotFound("User.NotFound", "This user could not be found");

    public static Error BadCredentials =>
        Error.Unauthorized("User.BadCredentials", "Username/email/password is connect");

    public static Error SettingOnCooldown =>
        Error.Forbidden("User.Cooldown.Setting", "Cannot edit a setting, on cooldown");
}
