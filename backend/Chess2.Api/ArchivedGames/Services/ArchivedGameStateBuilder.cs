using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.ArchivedGames.Services;

public interface IArchivedGameStateBuilder
{
    GameState FromArchive(GameArchive archive);
}

public class ArchivedGameStateBuilder : IArchivedGameStateBuilder
{
    public GameState FromArchive(GameArchive archive)
    {
        var whitePlayerArchive = archive.WhitePlayer;
        var blackPlayerArchive = archive.BlackPlayer;
        var sortedMoves = archive.Moves.OrderBy(m => m.MoveNumber);

        TimeControlSettings timeControl = new(archive.BaseSeconds, archive.IncrementSeconds);
        var whitePlayer = CreatePlayerFromArchive(whitePlayerArchive);
        var blackPlayer = CreatePlayerFromArchive(blackPlayerArchive);
        var result = CreateResultDataFromArchive(archive);
        var moveHistory = sortedMoves
            .Select(m => new MoveSnapshot(m.EncodedMove, m.San, m.TimeLeft))
            .ToList();

        var clocks = new ClockSnapshot(
            whitePlayerArchive.FinalTimeRemaining,
            blackPlayerArchive.FinalTimeRemaining,
            LastUpdated: null
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
            UserId: playerArchive.UserId,
            Color: playerArchive.Color,
            UserName: playerArchive.UserName,
            CountryCode: playerArchive.CountryCode,
            Rating: playerArchive.NewRating
        );

    private static GameResultData CreateResultDataFromArchive(GameArchive archive) =>
        new(
            Result: archive.Result,
            ResultDescription: archive.ResultDescription,
            WhiteRatingChange: archive.WhitePlayer.RatingChange,
            BlackRatingChange: archive.BlackPlayer.RatingChange
        );
}
