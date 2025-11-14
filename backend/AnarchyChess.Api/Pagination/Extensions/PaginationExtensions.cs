using AnarchyChess.Api.Pagination.Models;

namespace AnarchyChess.Api.Pagination.Extensions;

public static class PaginationExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, PaginationQuery query)
    {
        int skip = Math.Max(0, query.Page) * query.PageSize;
        return source.Skip(skip).Take(query.PageSize);
    }
}
