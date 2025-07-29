using System.Text.Json.Serialization;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Users.DTOs;

[method: JsonConstructor]
public record PublicUser(
    string UserId,
    string? UserName = null,
    string? About = null,
    string? CountryCode = null
)
{
    public static PublicUser FromAuthed(AuthedUser user) =>
        new(user.Id, user.UserName, user.About, user.CountryCode);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PrivateUser), SessionUserType.Authed)]
[JsonDerivedType(typeof(GuestUser), SessionUserType.Guest)]
public abstract record SessionUser(string UserId)
{
    public abstract string Type { get; }
}

public record PrivateUser(
    string UserId,
    string? UserName = null,
    string? Email = null,
    string? About = null,
    string? CountryCode = null
) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Authed;

    public static PrivateUser FromAuthed(AuthedUser user) =>
        new(user.Id, user.UserName, user.Email, user.About, user.CountryCode);
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

public record ProfileEditRequest(string? About = null, string? CountryCode = null)
{
    public ProfileEditRequest(AuthedUser user)
        : this(user.About, user.CountryCode) { }
}
