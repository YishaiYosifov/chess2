namespace Chess2.Api.Matchmaking.Models;

public record SeekInfo(string UserId, int Rating, int WavesMissed = 0)
{
    public string UserId { get; } = UserId;
    public int Rating { get; init; } = Rating;
    public int WavesMissed { get; set; } = WavesMissed;
}
