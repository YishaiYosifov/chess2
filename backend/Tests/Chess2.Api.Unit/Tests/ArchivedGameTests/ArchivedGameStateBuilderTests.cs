using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ArchivedGameTests;

public class ArchivedGameStateBuilderTests
{
    private readonly ArchivedGameStateBuilder _stateBuilder;

    public ArchivedGameStateBuilderTests()
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
            .Select(m => new MoveSnapshot(MapMovePath(m), m.San, m.TimeLeft))
            .ToList();

        ClockSnapshot expectedClocks = new(
            whitePlayer.FinalTimeRemaining,
            blackPlayer.FinalTimeRemaining,
            LastUpdated: null
        );

        GameState expectedGameState = new(
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
            InitialFen: archive.InitialFen,
            MoveHistory: expectedMoveHistory,
            MoveOptions: new(),
            ResultData: new GameResultData(
                Result: archive.Result,
                ResultDescription: archive.ResultDescription,
                WhiteRatingChange: whitePlayer.RatingChange,
                BlackRatingChange: blackPlayer.RatingChange
            )
        );

        var result = _stateBuilder.FromArchive(archive);

        result.Should().BeEquivalentTo(expectedGameState);
    }

    private static MovePath MapMovePath(MoveArchive moveArchive)
    {
        List<MoveSideEffectPath> sideEffects = [];
        if (moveArchive.SideEffects is not null)
            sideEffects =
            [
                .. moveArchive.SideEffects.Select(se => new MoveSideEffectPath(
                    FromIdx: se.FromIdx,
                    ToIdx: se.ToIdx
                )),
            ];
        return new(
            FromIdx: moveArchive.FromIdx,
            ToIdx: moveArchive.ToIdx,
            CapturedIdxs: [.. moveArchive.Captures],
            TriggerIdxs: [.. moveArchive.Triggers],
            SideEffects: sideEffects
        );
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
