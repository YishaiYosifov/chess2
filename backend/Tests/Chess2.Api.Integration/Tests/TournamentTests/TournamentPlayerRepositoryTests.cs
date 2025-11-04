using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Tournaments.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.TournamentTests;

public class TournamentPlayerRepositoryTests : BaseIntegrationTest
{
    private readonly ITournamentPlayerRepository _repository;

    public TournamentPlayerRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<ITournamentPlayerRepository>();
    }

    [Fact]
    public async Task AddPlayerAsync_adds_player()
    {
        var player = new TournamentPlayerFaker().Generate();
        await _repository.AddPlayerAsync(player, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.TournamentPlayers.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task RemovePlayerFromTournamentAsync_removes_player_from_just_one_tournament()
    {
        var user = new AuthedUserFaker().Generate();
        var playerTournament1 = new TournamentPlayerFaker(user).Generate();
        var playerTournament2 = new TournamentPlayerFaker(user).Generate();
        var otherPlayerTournament1 = new TournamentPlayerFaker()
            .RuleFor(x => x.TournamentToken, playerTournament1.TournamentToken)
            .Generate();
        await DbContext.AddRangeAsync(
            user,
            playerTournament1,
            playerTournament2,
            otherPlayerTournament1
        );
        await DbContext.SaveChangesAsync(CT);

        await _repository.RemovePlayerFromTournamentAsync(
            playerTournament1.UserId,
            playerTournament1.TournamentToken,
            CT
        );
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.TournamentPlayers.AsNoTracking().ToListAsync(CT);
        inDb.Should().BeEquivalentTo([playerTournament2, otherPlayerTournament1]);
    }

    [Fact]
    public async Task GetAllPlayerofTournamentAsync_returns_only_players_for_tournament()
    {
        var user = new AuthedUserFaker().Generate();
        var player1 = new TournamentPlayerFaker(user).Generate();
        var player2 = new TournamentPlayerFaker()
            .RuleFor(x => x.TournamentToken, player1.TournamentToken)
            .Generate();
        var otherPlayer = new TournamentPlayerFaker(user).Generate();
        await DbContext.AddRangeAsync(user, player1, player2, otherPlayer);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetAllPlayersOfTournamentAsync(player1.TournamentToken, CT);

        result.Should().BeEquivalentTo([player1, player2]);
    }
}
