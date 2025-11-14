using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Challenges.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Challenges.Models.ChallengeToken")]
[JsonConverter(typeof(StructStringJsonConverter<ChallengeToken>))]
[TypeConverter(typeof(StructStringTypeConverter<ChallengeToken>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct ChallengeToken(string Value)
{
    public static implicit operator string(ChallengeToken id) => id.Value;

    public static implicit operator ChallengeToken(string value) => new(value);

    public static implicit operator ChallengeToken?(string? value) =>
        value is null ? null : new(value);

    public override string ToString() => Value;
}
