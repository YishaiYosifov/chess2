namespace Chess2.Api.Infrastructure;

public static class Streaming
{
    public const string StreamProvider = "InMemoryProvider";
}

public static class Storage
{
    public const string StorageProvider = "GameState";
}

public static class AuthPolicies
{
    public const string AuthedUser = "AuthedUser";
    public const string ActiveSession = "ActiveSession";
    public const string RefreshAccess = "RefreshAccess";
}
