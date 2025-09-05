using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Social.Entities;
using Chess2.Api.Social.Errors;
using Chess2.Api.Social.Repository;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Social.Services;

public interface IStarService
{
    Task<ErrorOr<Created>> AddStarAsync(
        UserId forUserId,
        UserId starredUserId,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> RemoveStarAsync(
        UserId forUserId,
        UserId starredUserId,
        CancellationToken token = default
    );
    Task<PagedResult<MinimalProfile>> GetStarredUsersAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<bool> HasStarredAsync(
        UserId userId,
        UserId starredUser,
        CancellationToken token = default
    );
    Task<int> GetStarsReceivedCountAsync(UserId starredUserId, CancellationToken token = default);
}

public class StarService(
    IStarRepository starRepository,
    UserManager<AuthedUser> userManager,
    IUnitOfWork unitOfWork
) : IStarService
{
    private readonly IStarRepository _starRepository = starRepository;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> HasStarredAsync(
        UserId userId,
        UserId starredUser,
        CancellationToken token = default
    )
    {
        var star = await _starRepository.GetStarAsync(userId, starredUser, token);
        return star is not null;
    }

    public async Task<PagedResult<MinimalProfile>> GetStarredUsersAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var starredUsers = await _starRepository.GetPaginatedStarsGivenAsync(
            forUser,
            pagination,
            token
        );
        var totalCount = await _starRepository.GetStarsGivenCount(forUser, token);

        var minimalProfiles = starredUsers
            .Select(requester => new MinimalProfile(requester))
            .ToList();

        return new(
            Items: minimalProfiles,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public Task<int> GetStarsReceivedCountAsync(
        UserId starredUserId,
        CancellationToken token = default
    ) => _starRepository.GetStarsReceivedCountAsync(starredUserId, token);

    public async Task<ErrorOr<Created>> AddStarAsync(
        UserId forUserId,
        UserId starredUserId,
        CancellationToken token = default
    )
    {
        if (forUserId == starredUserId)
            return SocialErrors.CannotStar;

        var existingStar = await _starRepository.GetStarAsync(forUserId, starredUserId, token);
        if (existingStar is not null)
            return SocialErrors.AlreadyStarred;

        var starredUser = await _userManager.FindByIdAsync(starredUserId);
        if (starredUser is null)
            return ProfileErrors.NotFound;

        StarredUser star = new()
        {
            UserId = forUserId,
            StarredUserId = starredUserId,
            Starred = starredUser,
        };
        await _starRepository.AddStarAsync(star, token);
        await _unitOfWork.CompleteAsync(token);

        return Result.Created;
    }

    public async Task<ErrorOr<Success>> RemoveStarAsync(
        UserId forUserId,
        UserId starredUserId,
        CancellationToken token = default
    )
    {
        var star = await _starRepository.GetStarAsync(forUserId, starredUserId, token);
        if (star is null)
            return SocialErrors.NotStarred;

        _starRepository.RemoveStar(star);
        await _unitOfWork.CompleteAsync(token);

        return Result.Success;
    }
}
