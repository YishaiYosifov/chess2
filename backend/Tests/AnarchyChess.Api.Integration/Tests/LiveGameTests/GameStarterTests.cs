using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class GameStarterTests : BaseIntegrationTest
{
    private readonly GameStarter _gameStarter;
    private readonly IGrainFactory _grainFactory;

    private readonly IRandomProvider _randomProviderMock = Substitute.For<IRandomProvider>();

    public GameStarterTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _grainFactory = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        _gameStarter = new(
            _grainFactory,
            Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>(),
            Scope.ServiceProvider.GetRequiredService<IRatingService>(),
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>(),
            Scope.ServiceProvider.GetRequiredService<IRandomCodeGenerator>(),
            _randomProviderMock
        );
    }

    [Theory]
    [InlineData(0, GameColor.White)]
    [InlineData(1, GameColor.Black)]
    public async Task StartGameWithRandomColors_assigns_colors_correctly(
        int randomValue,
        GameColor user1Color
    )
    {
        PoolKey poolKey = new(
            PoolType.Casual,
            new TimeControlSettings(BaseSeconds: 60, IncrementSeconds: 2)
        );
        GameSource gameSource = GameSource.Matchmaking;

        var user1 = new AuthedUserFaker().Generate();
        var user1Rating = new CurrentRatingFaker(user1, timeControl: TimeControl.Bullet).Generate();
        var user2 = new AuthedUserFaker().Generate();
        var user2Rating = new CurrentRatingFaker(user2, timeControl: TimeControl.Bullet).Generate();
        await DbContext.AddRangeAsync(user1, user1Rating, user2, user2Rating);
        await DbContext.SaveChangesAsync(CT);
        _randomProviderMock.Next(2).Returns(randomValue);

        var gameToken = await _gameStarter.StartGameWithRandomColorsAsync(
            user1.Id,
            user2.Id,
            poolKey,
            gameSource,
            CT
        );

        var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameToken);
        var stateResult = await gameGrain.GetStateAsync();

        stateResult.IsError.Should().BeFalse();
        var state = stateResult.Value;

        state.Pool.Should().Be(poolKey);
        state.GameSource.Should().Be(gameSource);
        if (user1Color is GameColor.White)
        {
            state.WhitePlayer.UserId.Should().Be(user1.Id);
            state.BlackPlayer.UserId.Should().Be(user2.Id);
        }
        else
        {
            state.WhitePlayer.UserId.Should().Be(user2.Id);
            state.BlackPlayer.UserId.Should().Be(user1.Id);
        }
    }

    [Fact]
    public async Task StartGameWithColorsAsync_assigns_colors_deterministically()
    {
        PoolKey poolKey = new(
            PoolType.Casual,
            new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 2)
        );
        GameSource gameSource = GameSource.Rematch;

        var whiteUser = new AuthedUserFaker().Generate();
        var whiteRating = new CurrentRatingFaker(
            whiteUser,
            timeControl: TimeControl.Blitz
        ).Generate();

        var blackUser = new AuthedUserFaker().Generate();
        var blackRating = new CurrentRatingFaker(
            blackUser,
            timeControl: TimeControl.Blitz
        ).Generate();
        await DbContext.AddRangeAsync(whiteUser, whiteRating, blackUser, blackRating);
        await DbContext.SaveChangesAsync(CT);

        var gameToken = await _gameStarter.StartGameWithColorsAsync(
            whiteUserId: whiteUser.Id,
            blackUserId: blackUser.Id,
            poolKey,
            gameSource,
            CT
        );

        var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameToken);
        var stateResult = await gameGrain.GetStateAsync();

        stateResult.IsError.Should().BeFalse();
        var state = stateResult.Value;

        state.Pool.Should().Be(poolKey);
        state.GameSource.Should().Be(gameSource);

        state.WhitePlayer.UserId.Should().Be(whiteUser.Id);
        state.BlackPlayer.UserId.Should().Be(blackUser.Id);
    }
}
