using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameStateBuilderTests
{
    private readonly GameStateBuilder _stateBuilder;

    public GameStateBuilderTests()
    {
        _stateBuilder = new();
    }

    [Fact]
    public void FromArchive_builds_the_correct_state()
    {
        var archive = new GameArchiveFaker(moveCount: 3).Generate();
        var whitePlayer = archive.WhitePlayer!;
        var blackPlayer = archive.BlackPlayer!;

        var expectedMoveHistory = archive
            .Moves.OrderBy(m => m.MoveNumber)
            .Select(m => new MoveSnapshot(m.EncodedMove, m.San, m.TimeLeft))
            .ToList();

        var expectedClocks = new ClockDto(
            whitePlayer.FinalTimeRemaining,
            blackPlayer.FinalTimeRemaining,
            LastUpdated: null
        );

        var expectedGameState = new GameState(
            TimeControl: new TimeControlSettings(archive.BaseSeconds, archive.IncrementSeconds),
            IsRated: archive.IsRated,
            WhitePlayer: new GamePlayer(
                whitePlayer.UserId,
                whitePlayer.Color,
                whitePlayer.UserName,
                whitePlayer.CountryCode,
                whitePlayer.NewRating
            ),
            BlackPlayer: new GamePlayer(
                blackPlayer.UserId,
                blackPlayer.Color,
                blackPlayer.UserName,
                blackPlayer.CountryCode,
                blackPlayer.NewRating
            ),
            Clocks: expectedClocks,
            SideToMove: GameColor.Black,
            Fen: archive.FinalFen,
            LegalMoves: [],
            MoveHistory: expectedMoveHistory,
            ResultData: new GameResultData(
                archive.Result,
                archive.ResultDescription,
                whitePlayer.NewRating - whitePlayer.InitialRating,
                blackPlayer.NewRating - blackPlayer.InitialRating
            )
        );

        var result = _stateBuilder.FromArchive(archive);

        result.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public void FromArchive_with_an_empty_move_list_sets_data_correctly()
    {
        var archive = new GameArchiveFaker(moveCount: 0);
        var result = _stateBuilder.FromArchive(archive);

        result.Clocks.LastUpdated.Should().Be(null);
        result.SideToMove.Should().Be(GameColor.White);
        result.MoveHistory.Should().BeEmpty();
    }
}
