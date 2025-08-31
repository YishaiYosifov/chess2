using System.Text.Json.Serialization;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.Profile.DTOs;

[method: JsonConstructor]
public record PublicUser(string UserId, string UserName, string About, string CountryCode)
{
    public PublicUser(AuthedUser user)
        : this(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            About: user.About,
            CountryCode: user.CountryCode
        ) { }
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
    long? UsernameLastChangedSeconds,
    string About,
    string CountryCode
) : SessionUser(UserId)
{
    public override string Type => SessionUserType.Authed;

    public PrivateUser(AuthedUser user)
        : this(
            UserId: user.Id,
            UserName: user.UserName ?? "Unknown",
            UsernameLastChangedSeconds: user.UsernameLastChanged is null
                ? null
                : new DateTimeOffset(user.UsernameLastChanged.Value).ToUnixTimeSeconds(),
            About: user.About,
            CountryCode: user.CountryCode
        ) { }
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
