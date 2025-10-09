using Chess2.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;
using System.Text.Json.Serialization;

namespace Chess2.Api.Profile.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Profile.Models.UserId")]
[JsonConverter(typeof(StringValueObjectConverter<UserId>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct UserId(string Value)
{
    public static UserId Guest() =>
        $"guest:{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    public static implicit operator string(UserId id) => id.Value;

    public static implicit operator UserId(string value) => new(value);

    public static implicit operator UserId?(string? value) => value is null ? null : new(value);

    public bool IsGuest => Value.StartsWith("guest:");
    public bool IsAuthed => !IsGuest;

    public override string ToString() => Value;
}
