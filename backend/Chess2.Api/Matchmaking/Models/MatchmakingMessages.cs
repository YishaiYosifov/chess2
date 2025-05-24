using Akka.Actor;
using Chess2.Api.Game.Models;

namespace Chess2.Api.Matchmaking.Models;

public interface IMatchmakingMessage
{
    public TimeControlInfo TimeControl { get; init; }
}

public static class MatchmakingCommands
{
    public record CreateSeek(string UserId, int Rating, TimeControlInfo TimeControl)
        : IMatchmakingMessage;

    public record CancelSeek(string UserId, TimeControlInfo TimeControl) : IMatchmakingMessage;

    public record MatchWave() : INotInfluenceReceiveTimeout;
}

public static class MatchmakingEvents
{
    public record MatchFound(string OpponentId);
}
