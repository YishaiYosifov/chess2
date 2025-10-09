namespace Chess2.Api.Infrastructure;

public static class Streaming
{
    public const string StreamProvider = "InMemoryProvider";
}

public static class StorageNames
{
    public const string PlayerSessionState = "playerSessionState";
    public const string ChallengeState = "challengeState";
    public const string QuestState = "questState";
    public const string GameState = "gameState";
}

public static class AuthPolicies
{
    public const string AuthedUser = "AuthedUser";
    public const string ActiveSession = "ActiveSession";
    public const string RefreshAccess = "RefreshAccess";
}
