using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGameStateBuilder
{
    GameState FromArchive(GameArchive archive);
}

public class GameStateBuilder : IGameStateBuilder
{
    public GameState FromArchive(GameArchive archive)
    {
        var whitePlayerArchive = archive.WhitePlayer;
        var blackPlayerArchive = archive.BlackPlayer;
        if (whitePlayerArchive is null || blackPlayerArchive is null)
            throw new InvalidOperationException("Missing player archive");

        var sortedMoves = archive.Moves.OrderBy(m => m.MoveNumber);

        TimeControlSettings timeControl = new(archive.BaseSeconds, archive.IncrementSeconds);
        var whitePlayer = CreatePlayerFromArchive(whitePlayerArchive);
        var blackPlayer = CreatePlayerFromArchive(blackPlayerArchive);
        var result = CreateResultDataFromArchive(archive);
        var moveHistory = sortedMoves
            .Select(m => new MoveSnapshot(m.EncodedMove, m.San, m.TimeLeft))
            .ToList();

        var clocks = new ClockDto(
            whitePlayerArchive.FinalTimeRemaining,
            blackPlayerArchive.FinalTimeRemaining,
            sortedMoves.LastOrDefault()?.PlayedAt.ToUnixTimeMilliseconds()
        );
        var sideToMove = moveHistory.Count % 2 == 0 ? GameColor.White : GameColor.Black;

        return new(
            TimeControl: timeControl,
            IsRated: archive.IsRated,
            WhitePlayer: whitePlayer,
            BlackPlayer: blackPlayer,
            Clocks: clocks,
            SideToMove: sideToMove,
            Fen: archive.FinalFen,
            LegalMoves: [],
            MoveHistory: moveHistory,
            ResultData: result
        );
    }

    private static GamePlayer CreatePlayerFromArchive(PlayerArchive playerArchive) =>
        new(
            playerArchive.UserId,
            playerArchive.Color,
            playerArchive.UserName,
            playerArchive.CountryCode,
            playerArchive.NewRating
        );

    private static GameResultData CreateResultDataFromArchive(GameArchive archive) =>
        new(
            archive.Result,
            archive.ResultDescription,
            archive.WhitePlayer?.NewRating - archive.WhitePlayer?.InitialRating,
            archive.BlackPlayer?.NewRating - archive.BlackPlayer?.InitialRating
        );
}
