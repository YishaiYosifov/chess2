namespace Chess2.Api.Matchmaking.Models;

public record SeekInfo(string UserId, int TimeControl, int Increment, long StartedAtTimestamp);
