using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Shared.Models;

namespace AnarchyChess.Api.Donations.Repositories;

public interface IDonationWallService
{
    Task<PagedResult<string>> GetLeaderboardAsync(
        PaginationQuery query,
        CancellationToken token = default
    );
}

public class DonationWallService(IDonationRepository donationRepository) : IDonationWallService
{
    private readonly IDonationRepository _donationRepository = donationRepository;

    public async Task<PagedResult<string>> GetLeaderboardAsync(
        PaginationQuery query,
        CancellationToken token = default
    )
    {
        var donations = await _donationRepository.GetPaginatedLeaderboardAsync(query, token);
        var totalCount = await _donationRepository.GetTotalCountAsync(token);
        return new(
            Items: donations.Select(x => x.Name),
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
