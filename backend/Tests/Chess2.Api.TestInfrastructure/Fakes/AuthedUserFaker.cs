using Bogus;
using Chess2.Api.Models.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class AuthedUserFaker : Faker<AuthedUser>
{
    public static readonly string Password = "TestPassword";

    // csharpier-ignore
    private readonly byte[] PasswordHash = [
        171, 142, 166, 22,
        88, 125, 26, 236,
        98, 47, 166, 186,
        152, 86, 97, 239,
        1, 220, 24, 117,
        84, 51, 164, 172,
        43, 149, 207, 5,
        234, 11, 174, 31];

    // csharpier-ignore
    private readonly byte[] PasswordSalt = [
        192, 47, 30, 58,
        210, 205, 97, 156,
        84, 171, 75, 101,
        120, 154, 27, 114];

    public AuthedUserFaker()
    {
        StrictMode(true)
            .RuleFor(x => x.AuthedUserId, 0)
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.CountryCode, "IL")
            .RuleFor(x => x.About, "")
            .RuleFor(x => x.PasswordHash, "")
            .RuleFor(x => x.PasswordSalt, PasswordSalt)
            .RuleFor(x => x.UsernameLastChanged, DateTime.UtcNow)
            .RuleFor(x => x.PasswordLastChanged, DateTime.UtcNow)
            .RuleFor(x => x.Ratings, []);
    }
}
