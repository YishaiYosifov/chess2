using System.Text.Json.Serialization;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Users.DTOs;

[method: JsonConstructor]
public record PublicUser(string UserId, string UserName, string About, string CountryCode)
{
    public static PublicUser FromAuthed(AuthedUser user) =>
        new(user.Id, user.UserName ?? "Unknown", user.About, user.CountryCode);
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
    string UserName,
    long UsernameLastChanged,
    string About,
    string CountryCode
) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Authed;

    public static PrivateUser FromAuthed(AuthedUser user) =>
        new(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            UsernameLastChanged: new DateTimeOffset(user.UsernameLastChanged).ToUnixTimeSeconds(),
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
