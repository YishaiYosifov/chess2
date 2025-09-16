using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Entities;
using Chess2.Api.Quests.Repositories;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Quests.Services;

public interface IQuestService
{
    Task<PagedResult<QuestPointsDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetQuestPointsAsync(UserId userId, CancellationToken token = default);
    Task<int> GetRankingAsync(UserId userId, CancellationToken token = default);
    Task<ErrorOr<Updated>> IncrementQuestPointsAsync(
        UserId userId,
        int points,
        CancellationToken token = default
    );
    Task ResetAllQuestPointsAsync(CancellationToken token = default);
}

public class QuestService(
    IQuestRepository questRepository,
    UserManager<AuthedUser> userManager,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IQuestService
{
    private readonly IQuestRepository _questRepository = questRepository;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<QuestPointsDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var questPoints = await _questRepository.GetPaginatedLeaderboardAsync(pagination, token);
        var totalCount = await _questRepository.GetTotalCountAsync(token);

        return new(
            Items: questPoints.Select(questPoint => new QuestPointsDto(
                new MinimalProfile(questPoint.User),
                questPoint.Points
            )),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<int> GetRankingAsync(UserId userId, CancellationToken token = default)
    {
        var questPoints = await _questRepository.GetUserPointsAsync(userId, token);

        var position = await _questRepository.GetRankingAsync(questPoints?.Points ?? 0, token);
        return position;
    }

    public async Task<int> GetQuestPointsAsync(UserId userId, CancellationToken token = default)
    {
        var questPoints = await _questRepository.GetUserPointsAsync(userId, token);
        return questPoints?.Points ?? 0;
    }

    public async Task<ErrorOr<Updated>> IncrementQuestPointsAsync(
        UserId userId,
        int incrementBy,
        CancellationToken token = default
    )
    {
        var today = _timeProvider.GetUtcNow().UtcDateTime;
        var userQuestPoints = await _questRepository.GetUserPointsAsync(userId, token);
        if (userQuestPoints is not null)
        {
            userQuestPoints.LastQuestAt = today;
            userQuestPoints.Points += incrementBy;
            await _unitOfWork.CompleteAsync(token);
            return Result.Updated;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ProfileErrors.NotFound;

        await _questRepository.AddQuestPointsAsync(
            new UserQuestPoints()
            {
                UserId = user.Id,
                User = user,
                Points = incrementBy,
                LastQuestAt = today,
            },
            token
        );
        await _unitOfWork.CompleteAsync(token);
        return Result.Updated;
    }

    public async Task ResetAllQuestPointsAsync(CancellationToken token = default)
    {
        _questRepository.DeleteAll();
        await _unitOfWork.CompleteAsync(token);
    }
}
