using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class PlayerRosterTests
{
    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    [Fact]
    public void InitializePlayers_sets_the_players_correctly()
    {
        var roster = new PlayerRoster();

        roster.InitializePlayers(_whitePlayer, _blackPlayer);

        roster.WhitePlayer.Should().BeEquivalentTo(_whitePlayer);
        roster.BlackPlayer.Should().BeEquivalentTo(_blackPlayer);
    }

    [Fact]
    public void WhitePlayer_throws_if_not_initialized()
    {
        var roster = new PlayerRoster();
        var act = () => _ = roster.WhitePlayer;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BlackPlayer_throws_if_not_initialized()
    {
        var roster = new PlayerRoster();
        var act = () => _ = roster.BlackPlayer;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TryGetPlayerById_returns_the_correct_player()
    {
        var roster = new PlayerRoster();
        roster.InitializePlayers(_whitePlayer, _blackPlayer);

        roster.TryGetPlayerById(_whitePlayer.UserId, out var foundWhite).Should().BeTrue();
        foundWhite.Should().BeEquivalentTo(_whitePlayer);

        roster.TryGetPlayerById(_blackPlayer.UserId, out var foundBlack).Should().BeTrue();
        foundBlack.Should().BeEquivalentTo(_blackPlayer);

        roster.TryGetPlayerById("notfound", out var notFound).Should().BeFalse();
        notFound.Should().BeNull();
    }

    [Fact]
    public void TryGetPlayerByColor_returns_the_correct_player()
    {
        var roster = new PlayerRoster();
        roster.InitializePlayers(_whitePlayer, _blackPlayer);

        var whiteResult = roster.GetPlayerByColor(GameColor.White);
        whiteResult.Should().BeEquivalentTo(_whitePlayer);

        var blackResult = roster.GetPlayerByColor(GameColor.Black);
        blackResult.Should().BeEquivalentTo(_blackPlayer);
    }
}
