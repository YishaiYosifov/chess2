using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.GameToken")]
[JsonConverter(typeof(StructStringJsonConverter<GameToken>))]
[TypeConverter(typeof(StructStringTypeConverter<GameToken>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct GameToken(string Value)
{
    public static implicit operator string(GameToken id) => id.Value;

    public static implicit operator GameToken(string value) => new(value);

    public override string ToString() => Value;
}
