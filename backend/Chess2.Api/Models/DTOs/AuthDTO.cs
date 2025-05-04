using Chess2.Api.Models.Entities;

namespace Chess2.Api.Models.DTOs;

public class Tokens
{
    public required string AccessToken { get; set; }
    public required int AccessTokenExpiresInSeconds { get; set; }

    public required string RefreshToken { get; set; }
}

public class AuthResponseDTO
{
    public required Tokens AuthTokens { get; set; }
    public required AuthedUser User { get; set; }
}
