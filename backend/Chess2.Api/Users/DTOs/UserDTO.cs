using System.Text.Json.Serialization;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Users.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PublicUser), SessionUserType.Authed)]
[JsonDerivedType(typeof(GuestUser), SessionUserType.Guest)]
public abstract record SessionUser(string UserId)
{
    public abstract string Type { get; }
}

public record PublicUser(string UserId, string UserName, string About, string CountryCode)
    : SessionUser(UserId)
{
    public override string Type => SessionUserType.Authed;

    public static PublicUser FromAuthed(AuthedUser user) =>
        new(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            About: user.About,
            CountryCode: user.CountryCode
        );
}

public record GuestUser(string UserId) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Guest;
}

public static class SessionUserType
{
    public const string Authed = "authed";
    public const string Guest = "guest";
}

public record ProfileEditRequest(string About, string CountryCode)
{
    public ProfileEditRequest(AuthedUser user)
        : this(user.About, user.CountryCode) { }
}
