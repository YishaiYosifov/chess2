using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MinimalProfileFaker : RecordFaker<MinimalProfile>
{
    public MinimalProfileFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Internet.UserName());
    }
}
