using Bogus;
using AnarchyChess.Api.Profile.DTOs;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class ProfileEditFaker : Faker<ProfileEditRequest>
{
    public ProfileEditFaker()
    {
        StrictMode(true);
        RuleFor(x => x.About, f => f.Lorem.Sentence());
        RuleFor(x => x.CountryCode, "US");
    }
}
