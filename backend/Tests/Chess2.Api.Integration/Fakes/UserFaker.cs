using Bogus;
using Chess2.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Fakes;

public class UserFaker : Faker<User>
{
    // TestPassword
    private readonly byte[] PasswordHash = [
        171, 142, 166, 22,
        88, 125, 26, 236,
        98, 47, 166, 186,
        152, 86, 97, 239,
        1, 220, 24, 117,
        84, 51, 164, 172,
        43, 149, 207, 5,
        234, 11, 174, 31];

    private readonly byte[] PasswordSalt = [
        192, 47, 30, 58,
        210, 205, 97, 156,
        84, 171, 75, 101,
        120, 154, 27, 114];

    public UserFaker()
    {
        StrictMode(true)
            .RuleFor(x => x.UserId, 0)
            .RuleFor(x => x.Username, f => f.Person.UserName)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.PasswordHash, PasswordHash)
            .RuleFor(x => x.PasswordSalt, PasswordSalt);
    }
}
