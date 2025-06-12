using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Shared.Extensions;

namespace Chess2.Api.Game.Actors;

public class PlayerRoster
{
    public required GamePlayer PlayerWhite { get; init; }
    public required GamePlayer PlayerBlack { get; init; }
    public required IReadOnlyDictionary<string, GamePlayer> IdToPlayer { get; init; }
    public required IReadOnlyDictionary<GameColor, GamePlayer> ColorToPlayer { get; init; }

    public required GameColor CurrentPlayerColor { get; set; }
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
        var playerWhite = new GamePlayer() { UserId = startGame.WhiteId, Color = GameColor.White };
        var playerBlack = new GamePlayer() { UserId = startGame.BlackId, Color = GameColor.Black };
        var idToPlayer = new Dictionary<string, GamePlayer>()
        {
            [playerWhite.UserId] = playerWhite,
            [playerBlack.UserId] = playerBlack,
        };
        var colorToPlayer = new Dictionary<GameColor, GamePlayer>()
        {
            [GameColor.White] = playerWhite,
            [GameColor.Black] = playerBlack,
        };

        var players = new PlayerRoster()
        {
            PlayerWhite = playerWhite,
            PlayerBlack = playerBlack,
            IdToPlayer = idToPlayer.AsReadOnly(),
            ColorToPlayer = colorToPlayer.AsReadOnly(),
            CurrentPlayerColor = GameColor.White,
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

        Receive<GameQueries.GetGameState>(getGameState =>
            HandleGetGameState(getGameState, players)
        );

        Receive<GameCommands.MovePiece>(movePiece => HandleMovePiece(movePiece, players));
    }

    private void HandleGetGameState(GameQueries.GetGameState getGameState, PlayerRoster players)
    {
        var player = players.IdToPlayer.GetValueOrDefault(getGameState.ForUserId);
        if (player is null)
        {
            _logger.Warning(
                "Could not find player {0} when trying to get state for game {1}",
                getGameState.ForUserId,
                _token
            );
            Sender.Tell(ErrorOrFacEx.From<GameEvents.GameStateEvent>(GameErrors.PlayerInvalid));
            return;
        }
        var legalMoves = _game.GetEncodedLegalMovesFor(player.Color);

        var gameStateDto = new GameStateDto(
            PlayerWhite: new GamePlayerDto(players.PlayerWhite),
            PlayerBlack: new GamePlayerDto(players.PlayerBlack),
            CurrentPlayerColor: players.CurrentPlayerColor,
            Fen: _game.Fen,
            MoveHistory: _game.EncodedMoveHistory,
            LegalMoves: legalMoves
        );

        Sender.Tell(new GameEvents.GameStateEvent(gameStateDto));
    }

    private void HandleMovePiece(GameCommands.MovePiece movePiece, PlayerRoster players)
    {
        var currentPlayerId = players.ColorToPlayer[players.CurrentPlayerColor]?.UserId;
        if (currentPlayerId != movePiece.UserId)
        {
            _logger.Warning(
                "User {0} attmpted to move a piece, but their id doesn't match the current player {1}",
                movePiece.UserId,
                currentPlayerId
            );
            Sender.Tell(ErrorOrFacEx.From<GameEvents.PieceMoved>(GameErrors.PlayerInvalid));
            return;
        }

        var moveResult = _game.MakeMove(movePiece.From, movePiece.To);
        if (moveResult.IsError)
        {
            Sender.Tell(ErrorOrFacEx.From<GameEvents.PieceMoved>(moveResult.Errors));
            return;
        }

        var newCurrentPlayerColor = players.CurrentPlayerColor.Invert();
        players.CurrentPlayerColor = newCurrentPlayerColor;

        Sender.Tell(
            new GameEvents.PieceMoved(
                EncodedMove: moveResult.Value,
                WhiteLegalMoves: _game.GetEncodedLegalMovesFor(GameColor.White),
                BlackLegalMoves: _game.GetEncodedLegalMovesFor(GameColor.Black),
                PlayerTurn: newCurrentPlayerColor
            )
        );
    }
}
