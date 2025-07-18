using Akka.Actor;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingCommand
{
    public TimeControlSettings TimeControl { get; init; }
}

public static class MatchmakingCommands
{
    public record CancelSeek(string UserId, TimeControlSettings TimeControl) : IMatchmakingCommand;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public interface ICreateSeekCommand : IMatchmakingCommand
{
    public string UserId { get; }
}

public static class RatedMatchmakingCommands
{
    public record CreateRatedSeek(string UserId, int Rating, TimeControlSettings TimeControl)
        : ICreateSeekCommand;
}

public static class CasualMatchmakingCommands
{
    public record CreateCasualSeek(string UserId, TimeControlSettings TimeControl)
        : ICreateSeekCommand;
}

public static class MatchmakingBroadcasts
{
    public record SeekCreated(string UserId);

    public record SeekCanceled(string UserId);
}

public static class MatchmakingEvents
{
    public record MatchFound(string GameToken);

    public record MatchFailed();
}
