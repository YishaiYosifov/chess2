using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/api/auth/register")]
    Task<ApiResponse<UserOut>> RegisterAsync([Body] UserIn userIn);
}
