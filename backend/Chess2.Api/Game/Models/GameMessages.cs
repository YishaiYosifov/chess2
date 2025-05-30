namespace Chess2.Api.Game.Models;

public interface IGameMessage
{
    public string GameToken { get; }
}

public class GameCommands
{
    public record StartGame(string GameToken, string UserId1, string UserId2) : IGameMessage;
}
