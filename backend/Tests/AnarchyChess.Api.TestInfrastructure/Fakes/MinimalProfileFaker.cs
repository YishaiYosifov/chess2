using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MinimalProfileFaker : RecordFaker<MinimalProfile>
{
    public MinimalProfileFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Internet.UserName());
    }
}
