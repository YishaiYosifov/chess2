using System.Text.Json.Serialization;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Profile.DTOs;

[method: JsonConstructor]
[GenerateSerializer]
[Alias("AnarchyChess.Api.Profile.DTOs.MinimalProfile")]
public record MinimalProfile(UserId UserId, string UserName)
{
    public MinimalProfile(AuthedUser user)
        : this(user.Id, user.UserName ?? "Unknown") { }

    public MinimalProfile(UserId userId, AuthedUser? user)
        : this(userId, user?.UserName ?? "Guest") { }
}
