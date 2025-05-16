﻿using Bogus;
using Chess2.Api.Models.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RefreshTokenFaker : Faker<RefreshToken>
{
    public RefreshTokenFaker(AuthedUser user)
    {
        StrictMode(true)
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.UserId, user.Id)
            .RuleFor(x => x.User, user)
            .RuleFor(x => x.Jti, f => f.Random.Guid().ToString())
            .RuleFor(x => x.IsRevoked, false)
            .RuleFor(x => x.ExpiresAt, f => f.Date.Future(refDate: DateTime.UtcNow));
    }
}
