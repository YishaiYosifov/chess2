using System.ComponentModel;
using Chess2.Api.Models.Entities;

namespace Chess2.Api.Models.DTOs;

public class SignupRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? CountryCode { get; set; }
}

public class SigninRequest
{
    public required string UsernameOrEmail { get; set; }
    public required string Password { get; set; }
}

[DisplayName("User")]
public class UserOut
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? About { get; set; }
    public string? CountryCode { get; set; }

    public UserOut() { }

    public UserOut(AuthedUser user)
    {
        UserId = user.Id;
        UserName = user.UserName;
        About = user.About;
        CountryCode = user.CountryCode;
    }
}

[DisplayName("PrivateUser")]
public class PrivateUserOut : UserOut
{
    public string? Email { get; set; }

    public PrivateUserOut() { }

    public PrivateUserOut(AuthedUser user)
        : base(user)
    {
        Email = user.Email;
    }
}

public class ProfileEditRequest
{
    public string? About { get; set; }
    public string? CountryCode { get; set; }

    public ProfileEditRequest() { }

    public ProfileEditRequest(AuthedUser user)
    {
        About = user.About;
        CountryCode = user.CountryCode;
    }
}
