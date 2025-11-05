namespace Chess2.Api.Infrastructure;

public static class Streaming
{
    public const string StreamProvider = "InMemoryProvider";
}

public static class StorageNames
{
    public const string PlayerSessionState = "PlayerSessionState";
    public const string MatchmakingState = "MatchmakingState";
    public const string ChallengeState = "ChallengeState";
    public const string RematchState = "RematchState";
    public const string QuestState = "QuestState";
    public const string GameState = "GameState";
}

public static class AuthPolicies
{
    public const string AuthedUser = "AuthedUser";
    public const string ActiveSession = "ActiveSession";
    public const string RefreshAccess = "RefreshAccess";
}
