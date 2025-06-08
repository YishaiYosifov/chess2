using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Actors;

public class PlayerRoster
{
    public required GamePlayer PlayerWhite { get; init; }
    public required GamePlayer PlayerBlack { get; init; }
    public required GameColor PlayerToMove { get; set; }
}

public class GameActor : ReceiveActor
{
    private readonly string _token;

    private readonly IGame _game;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public GameActor(string token, IGame game)
    {
        _token = token;
        _game = game;

        Become(WaitingForStart);
    }

    private void WaitingForStart()
    {
        Receive<GameCommands.StartGame>(HandleStartGame);

        Receive<GameQueries.GetGameStatus>(_ =>
        {
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.NotStarted), Self);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    private void HandleStartGame(GameCommands.StartGame startGame)
    {
        var playerWhite = new GamePlayer()
        {
            UserId = startGame.WhiteId,
            Color = GameColor.White,
            PlayerActor = startGame.WhiteActor,
        };
        var playerBlack = new GamePlayer()
        {
            UserId = startGame.BlackId,
            Color = GameColor.Black,
            PlayerActor = startGame.BlackActor,
        };
        var players = new PlayerRoster()
        {
            PlayerWhite = playerWhite,
            PlayerBlack = playerBlack,
            PlayerToMove = GameColor.White,
        };
        _game.InitializeGame();

        Sender.Tell(new GameEvents.GameStartedEvent());
        Become(() => Playing(players));
    }

    private void Playing(PlayerRoster players)
    {
        Receive<GameQueries.GetGameStatus>(_ =>
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.OnGoing))
        );

        Receive<GameQueries.GetGameState>(_ => HandleGetGameState(players));
    }

    private void HandleGetGameState(PlayerRoster players)
    {
        var gameStateDto = new GameStateDto(
            PlayerWhite: new GamePlayerDto(players.PlayerWhite),
            PlayerBlack: new GamePlayerDto(players.PlayerBlack),
            PlayerToMove: players.PlayerToMove,
            Fen: _game.Fen,
            FenHistory: _game.FenHistory,
            LegalMoves: _game.LegalMoves
        );

        Sender.Tell(new GameEvents.GameStateEvent(gameStateDto), Self);
    }

    protected override void PostStop()
    {
        _logger.Info("GameActor with token {0} stopped.", _token);
        base.PostStop();
    }
}
