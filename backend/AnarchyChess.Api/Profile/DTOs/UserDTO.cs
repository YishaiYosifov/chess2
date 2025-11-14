using System.Text.Json.Serialization;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Profile.DTOs;

[method: JsonConstructor]
public record PublicUser(
    UserId UserId,
    string UserName,
    string About,
    string CountryCode,
    DateTime CreatedAt
)
{
    public PublicUser(AuthedUser user)
        : this(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            About: user.About,
            CountryCode: user.CountryCode,
            CreatedAt: user.CreatedAt
        ) { }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PrivateUser), SessionUserType.Authed)]
[JsonDerivedType(typeof(GuestUser), SessionUserType.Guest)]
public abstract record SessionUser(UserId UserId)
{
    public abstract string Type { get; }
}

[method: JsonConstructor]
public record PrivateUser(
    UserId UserId,
    string UserName,
    string About,
    string CountryCode,
    DateTime CreatedAt,
    DateTime? UsernameLastChanged
) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Authed;

    public PrivateUser(AuthedUser user)
        : this(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            About: user.About,
            CountryCode: user.CountryCode,
            CreatedAt: user.CreatedAt,
            UsernameLastChanged: user.UsernameLastChanged
        ) { }
}

public record GuestUser(UserId UserId) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Guest;
}

public static class SessionUserType
{
    public const string Authed = "authed";
    public const string Guest = "guest";
}
