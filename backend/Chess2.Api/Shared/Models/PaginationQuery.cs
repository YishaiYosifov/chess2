using System.Text.Json.Serialization;

namespace Chess2.Api.Shared.Models;

public record PaginationQuery(int Page = 0, int PageSize = 20)
{
    [JsonIgnore]
    public int Skip => Math.Max(0, Page) * PageSize;
}
