using System.ComponentModel;
using Chess2.Api.Models.Entities;
using Newtonsoft.Json;

namespace Chess2.Api.Models.DTOs;

[DisplayName("User")]
[method: JsonConstructor]
public record UserOut(
    int UserId,
    string? UserName = null,
    string? About = null,
    string? CountryCode = null
)
{
    public UserOut(AuthedUser user)
        : this(user.Id, user.UserName, user.About, user.CountryCode) { }
}

[DisplayName("PrivateUser")]
[method: JsonConstructor]
public record PrivateUserOut(
    int UserId,
    string? UserName = null,
    string? Email = null,
    string? About = null,
    string? CountryCode = null
) : UserOut(UserId, UserName, About, CountryCode)
{
    public PrivateUserOut(AuthedUser user)
        : this(user.Id, user.UserName, user.Email, user.About, user.CountryCode) { }
}

public record ProfileEditRequest(string? About = null, string? CountryCode = null)
{
    public ProfileEditRequest(AuthedUser user)
        : this(user.About, user.CountryCode) { }
}
