using Bogus;
using Chess2.Api.Models.DTOs;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class SignupRequestFaker : Faker<SignupRequest>
{
    public SignupRequestFaker()
    {
        StrictMode(true)
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.Password, f => f.Internet.Password())
            .RuleFor(x => x.CountryCode, "IL");
    }
}
