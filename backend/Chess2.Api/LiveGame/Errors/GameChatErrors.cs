using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.LiveGame.Errors;

public static class GameChatErrors
{
    public static Error UserAlreadyJoined =>
        Error.Conflict(ErrorCodes.GameChatUserAlreadyJoined, "User already joined the game chat");

    public static Error UserNotInChat =>
        Error.NotFound(ErrorCodes.GameChatUserNotInChat, "User is not in the game chat");
}
