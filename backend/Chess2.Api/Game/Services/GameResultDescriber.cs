using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGameResultDescriber
{
    string Aborted(GameColor by);
    string Resignation(GameColor loser);
}

public class GameResultDescriber : IGameResultDescriber
{
    public string Aborted(GameColor by) => $"Game Aborted by {by}";

    public string Resignation(GameColor loser) => $"{loser.Invert()} Won by Resignation";
}
