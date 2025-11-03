using System.ComponentModel;
using System.Text.Json.Serialization;
using Chess2.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Chess2.Api.Tournaments.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.TournamentId")]
[JsonConverter(typeof(StructStringJsonConverter<TournamentToken>))]
[TypeConverter(typeof(StructStringTypeConverter<TournamentToken>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct TournamentToken(string Value)
{
    public static implicit operator string(TournamentToken id) => id.Value;

    public static implicit operator TournamentToken(string value) => new(value);

    public override string ToString() => Value;
}
