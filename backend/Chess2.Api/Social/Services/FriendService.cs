using Chess2.Api.Pagination.Models;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Social.Entities;
using Chess2.Api.Social.Errors;
using Chess2.Api.Social.Repository;
using ErrorOr;

namespace Chess2.Api.Social.Services;

public interface IFriendService
{
    Task<ErrorOr<Created>> AcceptFriendRequestAsync(
        AuthedUser requester,
        AuthedUser recipient,
        CancellationToken token = default
    );
    Task<PagedResult<MinimalProfile>> GetFriendRequestsAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<ErrorOr<Created>> RequestFriendAsync(
        AuthedUser requester,
        AuthedUser recipient,
        CancellationToken token = default
    );
}

public class FriendService(
    IFriendRepository friendRepository,
    IPreferenceService preferenceService,
    IUnitOfWork unitOfWork
) : IFriendService
{
    private readonly IFriendRepository _friendRepository = friendRepository;
    private readonly IPreferenceService _preferenceService = preferenceService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<MinimalProfile>> GetFriendRequestsAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var requesters = await _friendRepository.GetIncomingFriendRequestsAsync(
            forUser,
            pagination,
            token
        );
        var totalCount = await _friendRepository.GetIncomingFriendRequestCount(forUser, token);

        var minimalProfiles = requesters
            .Select(requester => new MinimalProfile(
                UserId: requester.Id,
                UserName: requester.UserName ?? "Unknown"
            ))
            .ToList();

        return new(
            Items: minimalProfiles,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<ErrorOr<Created>> RequestFriendAsync(
        AuthedUser requester,
        AuthedUser recipient,
        CancellationToken token = default
    )
    {
        var existingRequest = await _friendRepository.GetRequestBetweenAsync(
            requester.Id,
            recipient.Id,
            token
        );
        if (existingRequest is not null)
        {
            var existingRequestResult = await HandleExistingRequestAsync(
                existingRequest,
                requester,
                token
            );
            await _unitOfWork.CompleteAsync(token);
            return existingRequestResult;
        }

        var recipientPrefs = await _preferenceService.GetPreferencesAsync(recipient.Id, token);
        if (!recipientPrefs.AllowFriendRequests)
            return SocialErrors.NotAcceptingFriends;

        FriendRequest request = new()
        {
            RequesterUserId = requester.Id,
            Requester = requester,
            RecipientUserId = recipient.Id,
            Recipient = recipient,
        };

        await _friendRepository.AddFriendRequestAsync(request, token);
        await _unitOfWork.CompleteAsync(token);

        return Result.Created;
    }

    public async Task<ErrorOr<Created>> AcceptFriendRequestAsync(
        AuthedUser requester,
        AuthedUser recipient,
        CancellationToken token = default
    )
    {
        var request = await _friendRepository.GetRequestBetweenAsync(
            requester.Id,
            recipient.Id,
            token
        );
        if (request is null || request.RecipientUserId != recipient.Id)
            return SocialErrors.FriendNotRequested;

        await ConvertRequestToFriendAsync(request, token);
        await _unitOfWork.CompleteAsync(token);
        return Result.Created;
    }

    private async Task<ErrorOr<Created>> HandleExistingRequestAsync(
        FriendRequest existing,
        AuthedUser requester,
        CancellationToken token = default
    )
    {
        if (existing.RequesterUserId == requester.Id)
            return SocialErrors.FriendAlreadyRequested;

        await ConvertRequestToFriendAsync(existing, token);
        return Result.Created;
    }

    private async Task ConvertRequestToFriendAsync(FriendRequest request, CancellationToken token)
    {
        Friend friend = new()
        {
            UserId1 = request.RequesterUserId,
            User1 = request.Requester,
            UserId2 = request.RecipientUserId,
            User2 = request.Recipient,
        };

        _friendRepository.DeleteFriendRequest(request);
        await _friendRepository.AddFriendAsync(friend, token);
    }
}
