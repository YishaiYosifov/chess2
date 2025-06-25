using Bogus;
using Chess2.Api.Game.Entities;
using Chess2.Api.GameLogic.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class PlayerArchiveFaker : Faker<PlayerArchive>
{
    public PlayerArchiveFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.Rating, f => f.Random.Int(1200, 2500));
        RuleFor(x => x.CountryCode, f => f.Address.CountryCode());
    }
}
