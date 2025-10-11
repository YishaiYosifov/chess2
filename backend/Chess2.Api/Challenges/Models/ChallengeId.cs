using Chess2.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Chess2.Api.Challenges.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Models.ChallengeId")]
[JsonConverter(typeof(StructStringJsonConverter<ChallengeId>))]
[TypeConverter(typeof(StructStringTypeConverter<ChallengeId>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct ChallengeId(string Value)
{
    public static implicit operator string(ChallengeId id) => id.Value;

    public static implicit operator ChallengeId(string value) => new(value);

    public static implicit operator ChallengeId?(string? value) =>
        value is null ? null : new(value);

    public override string ToString() => Value;
}
