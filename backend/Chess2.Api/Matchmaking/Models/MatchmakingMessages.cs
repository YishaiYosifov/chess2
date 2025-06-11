using Akka.Actor;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingCommand
{
    public PoolInfo PoolInfo { get; init; }
}

public static class MatchmakingCommands
{
    public record CancelSeek(string UserId, PoolInfo PoolInfo) : IMatchmakingCommand;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public interface ICreateSeekCommand : IMatchmakingCommand
{
    public string UserId { get; }
}

public static class RatedMatchmakingCommands
{
    public record CreateRatedSeek(string UserId, int Rating, PoolInfo PoolInfo)
        : ICreateSeekCommand;
}

public static class CasualMatchmakingCommands
{
    public record CreateCasualSeek(string UserId, PoolInfo PoolInfo) : ICreateSeekCommand;
}

public static class MatchmakingBroadcasts
{
    public record SeekCreated(string UserId);

    public record SeekCanceled(string UserId);
}

public static class MatchmakingEvents
{
    public record MatchFound(string GameToken);
}
