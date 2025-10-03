using System.Text.Json.Serialization;
using Chess2.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Chess2.Api.Profile.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Profile.Models.UserId")]
[JsonConverter(typeof(StringValueObjectConverter<UserId>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct UserId(string Value)
{
    public static implicit operator string(UserId id) => id.Value;

    public static implicit operator UserId(string value) => new(value);

    public static implicit operator UserId?(string? value) => value is null ? null : new(value);

    public override string ToString() => Value;
}
