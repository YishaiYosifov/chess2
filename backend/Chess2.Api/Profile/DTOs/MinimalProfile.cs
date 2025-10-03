using System.Text.Json.Serialization;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Profile.DTOs;

[method: JsonConstructor]
[GenerateSerializer]
[Alias("Chess2.Api.Profile.DTOs.MinimalProfile")]
public record MinimalProfile(UserId UserId, string UserName)
{
    public MinimalProfile(AuthedUser user)
        : this(user.Id, user.UserName ?? "Unknown") { }

    public MinimalProfile(UserId userId, AuthedUser? user)
        : this(userId, user?.UserName ?? "Guest") { }
}
