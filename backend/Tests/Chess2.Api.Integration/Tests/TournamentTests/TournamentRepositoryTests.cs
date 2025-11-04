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
}
