using Chess2.Api.Profile.Entities;
using Newtonsoft.Json;

namespace Chess2.Api.Profile.DTOs;

[method: JsonConstructor]
[GenerateSerializer]
[Alias("Chess2.Api.Profile.DTOs.MinimalProfile")]
public record MinimalProfile(string UserId, string UserName)
{
    public MinimalProfile(AuthedUser user)
        : this(user.Id, user.UserName ?? "Unknown") { }
}
