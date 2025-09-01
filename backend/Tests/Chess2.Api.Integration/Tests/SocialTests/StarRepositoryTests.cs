using Chess2.Api.Pagination.Models;
using Chess2.Api.Social.Repository;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.SocialTests;

public class StarRepositoryTests : BaseIntegrationTest
{
    private readonly IStarRepository _repository;

    private const string UserId = "test user";

    public StarRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IStarRepository>();
    }

    [Fact]
    public async Task GetPaginatedStarsAsync_applies_pagination()
    {
        var starredUsers = new StarredUserFaker(forUser: UserId).Generate(5);

        await DbContext.AddRangeAsync(starredUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedStarsAsync(
            UserId,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(starredUsers.Skip(2).Take(2).Select(s => s.Starred));
    }

    [Fact]
    public async Task GetStarredCountAsync_returns_correct_number()
    {
        var starredUsers = new StarredUserFaker(forUser: UserId).Generate(4);
        var otherStarred = new StarredUserFaker().Generate();

        await DbContext.AddRangeAsync(starredUsers, CT);
        await DbContext.AddRangeAsync(otherStarred);
        await DbContext.SaveChangesAsync(CT);

        var count = await _repository.GetStarredCountAsync(UserId, CT);

        count.Should().Be(starredUsers.Count);
    }

    [Fact]
    public async Task GetStarAsync_returns_star_if_exists()
    {
        var starredUser = new StarredUserFaker().Generate();
        var otherStar = new StarredUserFaker().Generate();

        await DbContext.AddRangeAsync(starredUser, otherStar);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetStarAsync(
            starredUser.UserId,
            starredUser.StarredUserId,
            CT
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(starredUser);
    }

    [Fact]
    public async Task GetStarAsync_returns_null_if_no_star_exists()
    {
        var starredUser = new StarredUserFaker().Generate();
        await DbContext.AddAsync(starredUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetStarAsync(starredUser.UserId, "nonexistent-user", CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddStarAsync_adds_star_to_db_context()
    {
        var starredUser = new StarredUserFaker().Generate();

        await _repository.AddStarAsync(starredUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbStar = await DbContext.StarredUsers.AsNoTracking().SingleOrDefaultAsync(CT);

        dbStar.Should().NotBeNull();
        dbStar.Should().BeEquivalentTo(starredUser);
    }

    [Fact]
    public async Task RemoveStar_removes_star_from_db_context()
    {
        var starToDelete = new StarredUserFaker().Generate();
        var otherStar = new StarredUserFaker().Generate();

        await DbContext.AddRangeAsync(starToDelete, otherStar);
        await DbContext.SaveChangesAsync(CT);

        _repository.RemoveStar(starToDelete);
        await DbContext.SaveChangesAsync(CT);

        var dbStars = await DbContext.StarredUsers.AsNoTracking().ToListAsync(CT);
        dbStars.Should().ContainSingle().Which.Should().BeEquivalentTo(otherStar);
    }
}
