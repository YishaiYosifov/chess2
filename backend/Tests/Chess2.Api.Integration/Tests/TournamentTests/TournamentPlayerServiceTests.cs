using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;
using Chess2.Api.Tournaments.Repositories;
using Chess2.Api.Tournaments.Services;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.TournamentTests;

public class TournamentPlayerServiceTests : BaseIntegrationTest
{
    private readonly TournamentPlayerService _playerService;

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    public TournamentPlayerServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _playerService = new TournamentPlayerService(
            Scope.ServiceProvider.GetRequiredService<ITournamentPlayerRepository>(),
            Scope.ServiceProvider.GetRequiredService<IRatingService>(),
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>(),
            Scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            _timeProviderMock
        );
    }

    [Fact]
    public async Task AddPlayerAsync_adds_correct_player()
    {
        var tournament = new TournamentFaker().RuleFor(x => x.BaseSeconds, 10).Generate();
        var user = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(user, timeControl: TimeControl.Bullet).Generate();
        await DbContext.AddRangeAsync(tournament, user, rating);
        await DbContext.SaveChangesAsync(CT);

        var result = await _playerService.AddPlayerAsync(user, tournament, CT);

        TournamentPlayer expectedPlayer = new()
        {
            UserId = user.Id,
            User = user,
            TournamentToken = tournament.TournamentToken,
            Rating = rating.Value,
        };

        var expectedResult = CreateTournamentPlayerState(expectedPlayer, rating);
        result.Should().BeEquivalentTo(expectedResult);

        var inDb = await DbContext.TournamentPlayers.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedPlayer, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task IncrementScoreForAsync_increments_and_saves()
    {
        var player = new TournamentPlayerFaker().Generate();
        await DbContext.AddAsync(player, CT);
        await DbContext.SaveChangesAsync(CT);

        await _playerService.IncrementScoreForAsync(player.UserId, player.TournamentToken, 100, CT);

        var inDb = await DbContext.TournamentPlayers.AsNoTracking().ToListAsync(CT);
        player.Score += 100;
        inDb.Should().BeEquivalentTo([player]);
    }

    [Fact]
    public async Task RemovePlayerAsync_removes_and_saves()
    {
        var player = new TournamentPlayerFaker().Generate();
        await DbContext.AddAsync(player, CT);
        await DbContext.SaveChangesAsync(CT);

        await _playerService.RemovePlayerAsync(player.UserId, player.TournamentToken, CT);

        var inDb = await DbContext.TournamentPlayers.AsNoTracking().ToListAsync(CT);
        inDb.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTournamentPlayersAsync_creates_the_correct_player_states()
    {
        var tournament = new TournamentFaker().Generate();
        var player1 = new TournamentPlayerFaker(tournament: tournament)
            .RuleFor(x => x.Score, 100)
            .RuleFor(x => x.LastOpponent, UserId.Authed())
            .Generate();
        var player1Rating = new CurrentRatingFaker(
            player1.User,
            timeControl: TimeControl.Classical
        ).Generate();

        var player2 = new TournamentPlayerFaker(tournament: tournament)
            .RuleFor(x => x.Score, 200)
            .Generate();
        var player2Rating = new CurrentRatingFaker(
            player1.User,
            timeControl: TimeControl.Classical
        ).Generate();

        var otherPlayer = new TournamentPlayerFaker().Generate();
        await DbContext.AddRangeAsync(
            tournament,
            player1,
            player1Rating,
            player2,
            player2Rating,
            otherPlayer
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _playerService.GetTournamentPlayersAsync(tournament, CT);

        result
            .Should()
            .BeEquivalentTo(
                new Dictionary<UserId, TournamentPlayerState>()
                {
                    [player1.UserId] = CreateTournamentPlayerState(player1, player1Rating),
                    [player2.UserId] = CreateTournamentPlayerState(player2, player2Rating),
                }
            );
    }

    private TournamentPlayerState CreateTournamentPlayerState(
        TournamentPlayer player,
        CurrentRating rating
    )
    {
        SeekerRating seekerRating = new(
            Value: rating.Value,
            AllowedRatingRange: null,
            rating.TimeControl
        );
        RatedSeeker seeker = new(
            UserId: player.UserId,
            UserName: player.User.UserName!,
            ExcludeUserIds: player.LastOpponent is null ? [] : [player.LastOpponent.Value],
            CreatedAt: _fakeNow,
            Rating: seekerRating
        );

        return new(seeker, player.Score);
    }
}
