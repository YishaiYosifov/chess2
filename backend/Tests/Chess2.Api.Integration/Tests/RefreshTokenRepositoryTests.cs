using Chess2.Api.Auth.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.RepositoryTests;

public class RefreshTokenRepositoryTests : BaseIntegrationTest
{
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
    }

    [Fact]
    public async Task GetTokenByJtiAsync_finds_the_correct_token()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var refreshTokenToFind = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RefreshTokenFaker(user)
        );
        await FakerUtils.StoreFakerAsync(DbContext, new RefreshTokenFaker(user));

        var result = await _repository.GetTokenByJtiAsync(refreshTokenToFind.Jti, CT);

        result.Should().BeEquivalentTo(refreshTokenToFind);
    }

    [Fact]
    public async Task GetTokenByJtiAsync_returns_null_when_token_is_not_found()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(DbContext, new RefreshTokenFaker(user));

        var result = await _repository.GetTokenByJtiAsync("some jti", CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddRefreshTokenAsync_insers_the_refresh_token()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var refreshToken = new RefreshTokenFaker(user).Generate();

        await _repository.AddRefreshTokenAsync(refreshToken, CT);
        await DbContext.SaveChangesAsync(CT);

        var fromDb = await DbContext.RefreshTokens.FindAsync([refreshToken.Id], CT);
        fromDb.Should().BeEquivalentTo(refreshToken);
    }
}
