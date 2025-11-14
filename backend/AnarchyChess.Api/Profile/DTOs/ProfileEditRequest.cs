using AnarchyChess.Api.Profile.Entities;

namespace AnarchyChess.Api.Profile.DTOs;

public record ProfileEditRequest(string About, string CountryCode)
{
    public void ApplyTo(AuthedUser user)
    {
        user.About = About;
        user.CountryCode = CountryCode;
    }
}
