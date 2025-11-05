using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Tournaments.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.TournamentTests;

public class TournamentRepositoryTests : BaseIntegrationTest
{
    private readonly ITournamentRepository _repository;

    public TournamentRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<ITournamentRepository>();
    }

    [Fact]
    public async Task AddTournamentAsync_adds_tournament()
    {
        var tournament = new TournamentFaker().Generate();
        await _repository.AddTournamentAsync(tournament, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.Tournaments.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(tournament);
    }

    [Fact]
    public async Task GetByTokenAsync_returns_correct_tournament()
    {
        var tournamentToFind = new TournamentFaker().Generate();
        var anotherTournament = new TournamentFaker().Generate();
        await DbContext.AddRangeAsync(tournamentToFind, anotherTournament);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetByTokenAsync(tournamentToFind.TournamentToken, CT);

        result.Should().BeEquivalentTo(tournamentToFind);
    }

    [Fact]
    public async Task UpdateTournament_starts_tracking_tournament()
    {
        var tournament = new TournamentFaker().RuleFor(x => x.HasStarted, false).Generate();
        await DbContext.AddAsync(tournament, CT);
        await DbContext.SaveChangesAsync(CT);

        DbContext.Entry(tournament).State = EntityState.Detached;

        tournament.HasStarted = true;

        _repository.UpdateTournament(tournament);

        DbContext.Entry(tournament).State.Should().Be(EntityState.Modified);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.Tournaments.AsNoTracking().SingleAsync(CT);
        inDb.HasStarted.Should().Be(true);
    }
}
