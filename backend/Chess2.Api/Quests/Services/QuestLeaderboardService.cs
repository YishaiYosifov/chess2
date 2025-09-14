using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Repositories;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Quests.Services;

public interface IQuestLeaderboardService
{
    Task<PagedResult<QuestPointsDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(AuthedUser user, CancellationToken token = default);
}

public class QuestLeaderboardService(IQuestLeaderboardRepository questLeaderboardRepository)
    : IQuestLeaderboardService
{
    private readonly IQuestLeaderboardRepository _questLeaderboardRepository =
        questLeaderboardRepository;

    public async Task<PagedResult<QuestPointsDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var users = await _questLeaderboardRepository.GetPaginatedLeaderboardAsync(
            pagination,
            token
        );
        var totalCount = await _questLeaderboardRepository.GetTotalCountAsync(token);

        return new(
            Items: users.Select(user => new QuestPointsDto(
                new MinimalProfile(user),
                user.QuestPoints
            )),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public Task<int> GetRankingAsync(AuthedUser user, CancellationToken token = default) =>
        _questLeaderboardRepository.GetRankingAsync(user, token);
}
