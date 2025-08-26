using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Users.Errors;

public static class UserErrors
{
    public static Error NotFound =>
        Error.NotFound(ErrorCodes.UserNotFound, "This user could not be found");

    public static Error SettingOnCooldown =>
        Error.Forbidden(ErrorCodes.UserSettingOnCooldown, "Cannot edit a setting, on cooldown");

    public static Error InvalidProfilePicture =>
        Error.Validation(
            ErrorCodes.UserInvalidProfilePicture,
            "The provided profile picture is not a valid image"
        );
}
