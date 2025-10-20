using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MinimalProfileFaker : RecordFaker<MinimalProfile>
{
    public MinimalProfileFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Internet.UserName());
    }
}
