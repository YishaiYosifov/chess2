﻿using Chess2.Api.Pagination.Models;
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

public interface IBlockService
{
    Task<ErrorOr<Created>> BlockUserAsync(
        UserId userId,
        UserId userIdToBlock,
        CancellationToken token = default
    );
    Task<HashSet<string>> GetAllBlockedUserIdsAsync(
        UserId forUser,
        CancellationToken token = default
    );
    Task<PagedResult<MinimalProfile>> GetBlockedUsersAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<bool> HasBlockedAsync(
        UserId byUserId,
        UserId blockedUserId,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> UnblockUserAsync(
        UserId userId,
        UserId userIdToUnblock,
        CancellationToken token = default
    );
}

public class BlockService(
    IBlockRepository blockRepository,
    UserManager<AuthedUser> userManager,
    IUnitOfWork unitOfWork
) : IBlockService
{
    private readonly IBlockRepository _blockRepository = blockRepository;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<MinimalProfile>> GetBlockedUsersAsync(
        UserId forUser,
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var blockedUsers = await _blockRepository.GetPaginatedBlockedUsersAsync(
            forUser,
            pagination,
            token
        );
        var totalCount = await _blockRepository.GetBlockedCountAsync(forUser, token);

        var minimalProfiles = blockedUsers
            .Select(requester => new MinimalProfile(requester))
            .ToList();

        return new(
            Items: minimalProfiles,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public Task<HashSet<string>> GetAllBlockedUserIdsAsync(
        UserId forUser,
        CancellationToken token = default
    ) => _blockRepository.GetAllBlockedUserIdsAsync(forUser, token);

    public async Task<bool> HasBlockedAsync(
        UserId byUserId,
        UserId blockedUserId,
        CancellationToken token = default
    ) => await _blockRepository.GetBlockedUserAsync(byUserId, blockedUserId, token) is not null;

    public async Task<ErrorOr<Created>> BlockUserAsync(
        UserId userId,
        UserId userIdToBlock,
        CancellationToken token = default
    )
    {
        if (userId == userIdToBlock)
            return SocialErrors.CannotBlockSelf;

        if (await HasBlockedAsync(userId, userIdToBlock, token))
            return SocialErrors.AlreadyBlocked;

        var userToBlock = await _userManager.FindByIdAsync(userIdToBlock);
        if (userToBlock is null)
            return ProfileErrors.NotFound;

        BlockedUser blocked = new()
        {
            UserId = userId,
            BlockedUserId = userToBlock.Id,
            Blocked = userToBlock,
        };
        await _blockRepository.AddBlockedUserAsync(blocked, token);
        await _unitOfWork.CompleteAsync(token);

        return Result.Created;
    }

    public async Task<ErrorOr<Success>> UnblockUserAsync(
        UserId userId,
        UserId userIdToUnblock,
        CancellationToken token = default
    )
    {
        var block = await _blockRepository.GetBlockedUserAsync(userId, userIdToUnblock, token);
        if (block is null)
            return SocialErrors.NotBlocked;

        _blockRepository.RemoveBlockedUser(block);
        await _unitOfWork.CompleteAsync(token);
        return Result.Success;
    }
}
