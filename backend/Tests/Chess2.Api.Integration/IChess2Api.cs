using Refit;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/auth/register")]
    Task Register([Body] UserLogin userLogin);
}
