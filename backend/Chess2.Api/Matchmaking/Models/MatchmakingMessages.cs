using Akka.Actor;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingCommand
{
    public PoolInfo PoolInfo { get; init; }
}

public static class MatchmakingCommands
{
    public record CreateRatedSeek(string UserId, int Rating, PoolInfo PoolInfo)
        : IMatchmakingCommand;

    public record CreateCasualSeek(string UserId, PoolInfo PoolInfo) : IMatchmakingCommand;

    public record CancelSeek(string UserId, PoolInfo PoolInfo) : IMatchmakingCommand;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public static class MatchmakingEvents
{
    public record MatchFound(string OpponentId);
}
