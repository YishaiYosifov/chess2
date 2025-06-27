using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameArchiveTests;

public class GameArchiveServiceTests : BaseIntegrationTest
{
    private readonly IGameArchiveService _gameArchiveService;

    public GameArchiveServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameArchiveService = Scope.ServiceProvider.GetRequiredService<IGameArchiveService>();
    }

    [Fact]
    public async Task CreateArchiveAsync_ShouldPersistGameArchiveCorrectly()
    {
        var gameToken = Guid.NewGuid().ToString("N")[..16];
        var gameState = new GameState(
            Fen: "10/10/10/10/10/10/10/10/10/10",
            WhitePlayer: new GamePlayerFaker(GameColor.White).Generate(),
            BlackPlayer: new GamePlayerFaker(GameColor.Black).Generate(),
            MoveHistory: ["e2e4", "e7e5", "g1f3"],
            SideToMove: GameColor.Black,
            LegalMoves: [],
            TimeControl: new(600, 5)
        );

        var result = await _gameArchiveService.CreateArchiveAsync(
            gameToken,
            gameState,
            GameResult.WhiteWin,
            CT
        );
        await DbContext.SaveChangesAsync(CT);

        var savedArchive = await DbContext
            .GameArchives.Include(g => g.Moves)
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .FirstOrDefaultAsync(g => g.GameToken == gameToken, CT);

        savedArchive.Should().NotBeNull();
        savedArchive.Should().BeEquivalentTo(result);

        savedArchive.WhitePlayer.Should().NotBeNull();
        savedArchive.BlackPlayer.Should().NotBeNull();
        AssertPlayerMatchingArchive(gameState.WhitePlayer, savedArchive.WhitePlayer);
        AssertPlayerMatchingArchive(gameState.BlackPlayer, savedArchive.BlackPlayer);

        savedArchive.FinalFen.Should().Be(gameState.Fen);
        savedArchive
            .Moves.Select(x => x.EncodedMove)
            .Should()
            .BeEquivalentTo(gameState.MoveHistory);
    }

    private void AssertPlayerMatchingArchive(GamePlayer player, PlayerArchive archive)
    {
        archive.UserId.Should().Be(player.UserId);
        archive.UserName.Should().Be(player.UserName);
        archive.Color.Should().Be(player.Color);
        archive.CountryCode.Should().Be(player.CountryCode);
        archive.Rating.Should().Be(player.Rating);
    }
}
