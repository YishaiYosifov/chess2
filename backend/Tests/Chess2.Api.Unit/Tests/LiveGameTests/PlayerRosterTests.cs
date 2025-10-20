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
    private readonly PlayerRoster _playerRoster;

    public PlayerRosterTests()
    {
        _playerRoster = new(_whitePlayer, _blackPlayer);
    }

    [Fact]
    public void TryGetPlayerById_returns_the_correct_player()
    {
        _playerRoster.TryGetPlayerById(_whitePlayer.UserId, out var foundWhite).Should().BeTrue();
        foundWhite.Should().Be(_whitePlayer);

        _playerRoster.TryGetPlayerById(_blackPlayer.UserId, out var foundBlack).Should().BeTrue();
        foundBlack.Should().Be(_blackPlayer);

        _playerRoster.TryGetPlayerById("notfound", out var notFound).Should().BeFalse();
        notFound.Should().BeNull();
    }

    [Fact]
    public void GetPlayerById_returns_the_correct_player()
    {
        _playerRoster.GetPlayerById(_whitePlayer.UserId).Should().Be(_whitePlayer);
        _playerRoster.GetPlayerById(_blackPlayer.UserId).Should().Be(_blackPlayer);
        _playerRoster.GetPlayerById("notfound").Should().BeNull();
    }

    [Fact]
    public void GetPlayerByColor_returns_the_correct_player()
    {
        _playerRoster.GetPlayerByColor(GameColor.White).Should().Be(_whitePlayer);
        _playerRoster.GetPlayerByColor(GameColor.Black).Should().Be(_blackPlayer);
    }
}
