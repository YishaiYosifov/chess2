using Chess2.Api.Users.Entities;

namespace Chess2.Api.Users.DTOs;

public record ProfileEditRequest(string About, string CountryCode)
{
    public void ApplyTo(AuthedUser user)
    {
        user.About = About;
        user.CountryCode = CountryCode;
    }
}
