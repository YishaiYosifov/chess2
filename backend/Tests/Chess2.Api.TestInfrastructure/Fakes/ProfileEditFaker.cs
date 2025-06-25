using Bogus;
using Chess2.Api.Users.DTOs;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class ProfileEditFaker : Faker<ProfileEditRequest>
{
    public ProfileEditFaker()
    {
        StrictMode(true);
        RuleFor(x => x.About, f => f.Lorem.Sentence());
        RuleFor(x => x.CountryCode, "US");
    }
}
