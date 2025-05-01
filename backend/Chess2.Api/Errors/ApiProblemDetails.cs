using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Errors;

public class ApiProblemDetails : ProblemDetails
{
    public IEnumerable<ApiProblemError> Errors { get; set; } = [];
}

public class ApiProblemError
{
    /// <see cref="ErrorCodes"/>
    public required string Code { get; set; }
    public required string Description { get; set; }
}

public static class ErrorCodes
{
    public const string UserUsernameConflict = "User.Conflict.Username";
    public const string UserEmailConflict = "User.Conflict.Username";
    public const string UserNotFound = "User.NotFound";
    public const string UserBadCredentials = "User.BadCredentials";
    public const string UserSettingOnCooldown = "User.Cooldown.Setting";

    public const string AuthTokenMissing = "Auth.TokenMissing";
    public const string AuthTokenInvalid = "Auth.TokenInvalid";
}
