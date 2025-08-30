using Chess2.Api.Profile.Entities;

namespace Chess2.Api.Profile.DTOs;

public record ProfileEditRequest(string About, string CountryCode)
{
    public void ApplyTo(AuthedUser user)
    {
        user.About = About;
        user.CountryCode = CountryCode;
    }
}
