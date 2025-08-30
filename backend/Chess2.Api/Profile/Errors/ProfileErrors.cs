using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Profile.Errors;

public static class ProfileErrors
{
    public static Error NotFound =>
        Error.NotFound(ErrorCodes.ProfileNotFound, "This user could not be found");

    public static Error SettingOnCooldown =>
        Error.Forbidden(ErrorCodes.ProfileSettingOnCooldown, "Cannot edit a setting, on cooldown");

    public static Error InvalidProfilePicture =>
        Error.Validation(
            ErrorCodes.ProfileInvalidProfilePicture,
            "The provided profile picture is not a valid image"
        );

    public static Error FriendAlreadyRequested =>
        Error.Conflict(
            ErrorCodes.ProfileFriendAlreadyRequested,
            "You already have an outgoing friend request to this user"
        );
}
