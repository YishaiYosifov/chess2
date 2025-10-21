using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Game.Errors;

public static class GameChatErrors
{
    public static Error InvalidUser =>
        Error.Forbidden(ErrorCodes.GameChatInvalidUser, "This user cannot send messages");

    public static Error InvalidMessage =>
        Error.Validation(ErrorCodes.GameChatInvalidMessage, "Message is invalid");

    public static Error OnCooldown =>
        Error.Validation(
            ErrorCodes.GameChatOnCooldown,
            "You are on cooldown and cannot send messages right now"
        );
}
