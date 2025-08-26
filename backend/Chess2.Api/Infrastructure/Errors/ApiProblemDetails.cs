using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Infrastructure.Errors;

public class ApiProblemDetails : ProblemDetails
{
    public IEnumerable<ApiProblemError> Errors { get; set; } = [];
}

public class ApiProblemError
{
    /// <see cref="ErrorCodes"/>
    public required string ErrorCode { get; set; }
    public required string Description { get; set; }
}

public static class ErrorCodes
{
    public const string UserNotFound = "User.NotFound";
    public const string UserSettingOnCooldown = "User.Cooldown.Setting";
    public const string UserInvalidProfilePicture = "User.InvalidProfilePicture";

    public const string AuthTokenMissing = "Auth.TokenMissing";
    public const string AuthTokenInvalid = "Auth.TokenInvalid";
    public const string AuthOAuthInvalid = "Auth.OAuth.Invalid";
    public const string AuthOAuthProviderNotFound = "Auth.OAuth.ProviderNotFound";

    public const string GameLogicPieceNotFound = "GameLogic.PieceNotFound";
    public const string GameLogicPointOutOfBound = "GameLogic.PointOutOfBound";

    public const string MatchmakingSeekNotFound = "Matchmaking.SeekNotFound";
    public const string MatchmakingSeekerNotCompatible = "Matchmaking.SeekerNotCompatible";

    public const string PlayerSessionConnectionInGame = "PlayerSession.ConnectionInGame";
    public const string PlayerSessionTooManyGames = "PlayerSession.TooManyGames";

    public const string GameNotFound = "Game.NotFound";
    public const string GameAlreadyEnded = "Game.AlreadyEnded";
    public const string GamePlayerInvalid = "Game.PlayerInvalid";
    public const string GameMoveInvalid = "Game.MoveInvalid";
    public const string GameDrawAlreadyRequested = "Game.DrawAlreadyRequested";
    public const string GameDrawOnCooldown = "Game.DrawOnCooldown";
    public const string GameDrawNotRequested = "Game.DrawNotRequested";

    public const string GameChatInvalidUser = "GameChat.InvalidUser";
    public const string GameChatInvalidMessage = "GameChat.InvalidMessage";
    public const string GameChatOnCooldown = "GameChat.OnCooldown";
}
