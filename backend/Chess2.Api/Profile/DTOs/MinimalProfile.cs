using Chess2.Api.Profile.Entities;
using System.Text.Json.Serialization;

namespace Chess2.Api.Profile.DTOs;

[method: JsonConstructor]
public record MinimalProfile(string UserId, string UserName)
{
    public MinimalProfile(AuthedUser user)
        : this(user.Id, user.UserName ?? "Unknown") { }
}
