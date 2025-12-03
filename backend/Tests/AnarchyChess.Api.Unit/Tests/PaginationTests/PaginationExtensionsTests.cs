using AnarchyChess.Api.Pagination.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.PaginationTests;

public class PaginationExtensionsTests
{
    private readonly IQueryable<int> _source = Enumerable.Range(1, 50).AsQueryable();

    [Fact]
    public void Paginate_returns_first_page_correctly()
    {
        PaginationQuery query = new(0, 10);

        var result = _source.Paginate(query).ToList();

        result.Should().HaveCount(10);
        result.Should().ContainInOrder(Enumerable.Range(1, 10));
    }

    [Fact]
    public void Paginate_returns_second_page_correctly()
    {
        PaginationQuery query = new(1, 10);

        var result = _source.Paginate(query).ToList();

        result.Should().HaveCount(10);
        result.Should().ContainInOrder(Enumerable.Range(11, 10));
    }

    [Fact]
    public void Paginate_skips_items_correctly()
    {
        PaginationQuery query = new(2, 5);

        var result = _source.Paginate(query).ToList();

        result.Should().HaveCount(5);
        result.Should().ContainInOrder(Enumerable.Range(11, 5));
    }

    [Fact]
    public void Paginate_handles_page_index_less_than_zero()
    {
        PaginationQuery query = new(-1, 10);

        var result = _source.Paginate(query).ToList();

        result.Should().HaveCount(10);
        result.Should().ContainInOrder(Enumerable.Range(1, 10));
    }

    [Fact]
    public void Paginate_handles_page_size_larger_than_source()
    {
        PaginationQuery query = new(0, 100);

        var result = _source.Paginate(query).ToList();

        result.Should().HaveCount(50);
        result.Should().ContainInOrder(Enumerable.Range(1, 50));
    }

    [Fact]
    public void Paginate_returns_empty_if_page_exceeds_source()
    {
        PaginationQuery query = new(10, 10);

        var result = _source.Paginate(query).ToList();

        result.Should().BeEmpty();
    }
}
