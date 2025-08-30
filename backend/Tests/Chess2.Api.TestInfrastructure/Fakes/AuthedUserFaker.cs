using Bogus;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class AuthedUserFaker : Faker<AuthedUser>
{
    public AuthedUserFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, f => f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Person.UserName);
        RuleFor(x => x.NormalizedUserName, (f, p) => p.UserName?.ToUpper());
        RuleFor(x => x.Email, f => f.Person.Email);
        RuleFor(x => x.NormalizedEmail, (f, p) => p.Email?.ToUpper());
        RuleFor(x => x.EmailConfirmed, true);
        RuleFor(x => x.SecurityStamp, f => f.Random.Guid().ToString());
        RuleFor(x => x.ConcurrencyStamp, f => f.Random.Guid().ToString());
        RuleFor(x => x.PhoneNumber, (string?)null);
        RuleFor(x => x.PhoneNumberConfirmed, false);
        RuleFor(x => x.TwoFactorEnabled, false);
        RuleFor(x => x.LockoutEnd, (DateTimeOffset?)null);
        RuleFor(x => x.LockoutEnabled, false);
        RuleFor(x => x.AccessFailedCount, 0);
        RuleFor(x => x.CountryCode, "IL");
        RuleFor(x => x.About, "");
        RuleFor(x => x.PasswordHash, (string?)null);
        RuleFor(x => x.UsernameLastChanged, DateTime.UtcNow);
    }
}
