using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Shared.Services;
using Chess2.Api.Social.Entities;
using Chess2.Api.Social.Errors;
using Chess2.Api.Social.Repository;
using ErrorOr;

namespace Chess2.Api.Social.Services;

public interface IFriendService
{
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
            var acceptResult = await TryAcceptRequestAsync(
                existingRequest,
                requester,
                recipient,
                token
            );
            await _unitOfWork.CompleteAsync(token);
            return acceptResult;
        }

        var recipientPrefs = await _preferenceService.GetPreferencesAsync(recipient.Id, token);
        if (!recipientPrefs.AllowFriendRequests)
            return SocialErrors.NotAcceptingFriends;

        FriendRequest request = new()
        {
            RequesterUserId = requester.Id,
            RecipientUserId = recipient.Id,
        };
        await _friendRepository.AddFriendRequestAsync(request, token);
        return Result.Created;
    }

    private async Task<ErrorOr<Created>> TryAcceptRequestAsync(
        FriendRequest request,
        AuthedUser requester,
        AuthedUser recipient,
        CancellationToken token = default
    )
    {
        if (request.RequesterUserId == requester.Id)
            return SocialErrors.FriendAlreadyRequested;

        Friend friend = new()
        {
            UserId1 = requester.Id,
            User1 = requester,
            UserId2 = recipient.Id,
            User2 = recipient,
        };

        _friendRepository.DeleteFriendRequest(request);
        await _friendRepository.AddFriendAsync(friend, token);
        return Result.Created;
    }
}
