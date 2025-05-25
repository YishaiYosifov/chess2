using Akka.Actor;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingMessage
{
    public PoolInfo PoolInfo { get; init; }
}

public static class MatchmakingCommands
{
    public record CreateRatedSeek(string UserId, int Rating, PoolInfo PoolInfo)
        : IMatchmakingMessage;

    public record CreateCasualSeek(string UserId, PoolInfo PoolInfo)
        : IMatchmakingMessage;

    public record CancelSeek(string UserId, PoolInfo PoolInfo) : IMatchmakingMessage;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public static class MatchmakingEvents
{
    public record MatchFound(string OpponentId);
}
