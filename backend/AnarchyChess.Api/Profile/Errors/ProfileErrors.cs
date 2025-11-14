using AnarchyChess.Api.Infrastructure.Errors;
using ErrorOr;

namespace AnarchyChess.Api.Profile.Errors;

public static class ProfileErrors
{
    public static Error NotFound =>
        Error.NotFound(ErrorCodes.ProfileNotFound, "This user could not be found");

    public static Error SettingOnCooldown =>
        Error.Forbidden(ErrorCodes.ProfileSettingOnCooldown, "Cannot edit a setting, on cooldown");

    public static Error UserNameTaken =>
        Error.Conflict(ErrorCodes.ProfileUserNameTaken, "Username Taken");

    public static Error InvalidProfilePicture =>
        Error.Validation(
            ErrorCodes.ProfileInvalidProfilePicture,
            "The provided profile picture is not a valid image"
        );
}
