using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Profile.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Profile.Models.UserId")]
[JsonConverter(typeof(StructStringJsonConverter<UserId>))]
[TypeConverter(typeof(StructStringTypeConverter<UserId>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct UserId(string Value)
{
    public static UserId Guest() =>
        $"guest:{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    public static UserId Authed() => Guid.NewGuid().ToString();

    public static implicit operator string(UserId id) => id.Value;

    public static implicit operator UserId(string value) => new(value);

    public static implicit operator UserId?(string? value) => value is null ? null : new(value);

    public bool IsGuest => Value.StartsWith("guest:");
    public bool IsAuthed => !IsGuest;

    public override string ToString() => Value;
}
