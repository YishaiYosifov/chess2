using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Social.Errors;

public static class SocialErrors
{
    public static Error FriendAlreadyRequested =>
        Error.Conflict(
            ErrorCodes.SocialFriendAlreadyRequested,
            "You already have an outgoing friend request to this user"
        );
    public static Error NotAcceptingFriends =>
        Error.Forbidden(
            ErrorCodes.SocialNotAcceptingFriends,
            "This user is not accepting friend requests"
        );
}
