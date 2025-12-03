using AnarchyChess.Api.Auth.Repositories;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests;

public class RefreshTokenRepositoryTests : BaseIntegrationTest
{
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
    }

    [Fact]
    public async Task GetTokenByJtiAsync_finds_the_correct_token()
    {
        var user = new AuthedUserFaker().Generate();
        var refreshTokenToFind = new RefreshTokenFaker(user).Generate();
        await DbContext.AddRangeAsync(
            user,
            refreshTokenToFind,
            new RefreshTokenFaker(user).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTokenByJtiAsync(refreshTokenToFind.Jti, CT);

        result.Should().BeEquivalentTo(refreshTokenToFind);
    }

    [Fact]
    public async Task GetTokenByJtiAsync_returns_null_when_token_is_not_found()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user, new RefreshTokenFaker(user).Generate());
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTokenByJtiAsync("some jti", CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddRefreshTokenAsync_inserts_the_refresh_token()
    {
        var user = new AuthedUserFaker().Generate();
        var refreshToken = new RefreshTokenFaker(user).Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await _repository.AddRefreshTokenAsync(refreshToken, CT);
        await DbContext.SaveChangesAsync(CT);

        var fromDb = await DbContext.RefreshTokens.FindAsync([refreshToken.Id], CT);
        fromDb.Should().BeEquivalentTo(refreshToken);
    }
}
