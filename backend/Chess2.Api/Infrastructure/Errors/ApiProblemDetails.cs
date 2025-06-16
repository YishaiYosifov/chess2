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

    public const string AuthTokenMissing = "Auth.TokenMissing";
    public const string AuthTokenInvalid = "Auth.TokenInvalid";
    public const string AuthOAuthInvalid = "Auth.OAuth.Invalid";
    public const string AuthOAuthProviderNotFound = "Auth.OAuth.ProviderNotFound";

    public const string GameLogicPieceNotFound = "GameLogic.PieceNotFound";
    public const string GameLogicPointOutOfBound = "GameLogic.PointOutOfBound";

    public const string GameNotFound = "Game.NotFound";
    public const string GamePlayerInvalid = "Game.PlayerInvalid";
    public const string GameMoveInvalid = "Game.MoveInvalid";
}
