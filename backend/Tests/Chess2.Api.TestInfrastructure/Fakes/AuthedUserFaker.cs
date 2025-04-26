using Bogus;
using Chess2.Api.Models.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class AuthedUserFaker : Faker<AuthedUser>
{
    public static readonly string Password = "TestPassword";

    // csharpier-ignore
    private const string PasswordHash = "AQAAAAIAAYagAAAAEA2CbBkJ8EhGQjLWKntGA/Rd57QYlcb3Myzv3qJ+O5gNNSGI/mKG83OiktTfuvj5RA==";

    public AuthedUserFaker()
    {
        StrictMode(true)
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.UserName, f => f.Person.UserName)
            .RuleFor(x => x.NormalizedUserName, (f, p) => p.UserName?.ToUpper())
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.NormalizedEmail, (f, p) => p.Email?.ToUpper())
            .RuleFor(x => x.EmailConfirmed, true)
            .RuleFor(x => x.SecurityStamp, (string?)null)
            .RuleFor(x => x.ConcurrencyStamp, (string?)null)
            .RuleFor(x => x.PhoneNumber, (string?)null)
            .RuleFor(x => x.PhoneNumberConfirmed, false)
            .RuleFor(x => x.TwoFactorEnabled, false)
            .RuleFor(x => x.LockoutEnd, (DateTimeOffset?)null)
            .RuleFor(x => x.LockoutEnabled, false)
            .RuleFor(x => x.AccessFailedCount, 0)
            .RuleFor(x => x.CountryCode, "IL")
            .RuleFor(x => x.About, "")
            .RuleFor(x => x.PasswordHash, PasswordHash)
            .RuleFor(x => x.UsernameLastChanged, DateTime.UtcNow)
            .RuleFor(x => x.PasswordLastChanged, DateTime.UtcNow)
            .RuleFor(x => x.Ratings, []);
    }
}
