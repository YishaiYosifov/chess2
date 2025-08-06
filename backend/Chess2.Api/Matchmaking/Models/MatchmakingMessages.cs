using Akka.Actor;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingCommand
{
    public PoolKey Key { get; }
}

public static class MatchmakingCommands
{
    public record CancelSeek(string UserId, PoolKey Key) : IMatchmakingCommand;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public interface ICreateSeekCommand : IMatchmakingCommand
{
    public string UserId { get; }
}

public static class RatedMatchmakingCommands
{
    public record CreateRatedSeek(string UserId, int Rating, TimeControlSettings TimeControl)
        : ICreateSeekCommand
    {
        public PoolKey Key { get; } = new(PoolType.Rated, TimeControl);
    }
}

public static class CasualMatchmakingCommands
{
    public record CreateCasualSeek(string UserId, TimeControlSettings TimeControl)
        : ICreateSeekCommand
    {
        public PoolKey Key { get; } = new(PoolType.Casual, TimeControl);
    }
}

public static class MatchmakingReplies
{
    public record SeekCreated(string UserId);

    public record SeekCanceled(string UserId);
}

public static class MatchmakingEvents
{
    public record MatchFound(string GameToken, PoolKey Key);

    public record MatchFailed();
}
