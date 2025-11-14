using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Shared.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Shared.Models.ConnectionId")]
[JsonConverter(typeof(StructStringJsonConverter<ConnectionId>))]
[TypeConverter(typeof(StructStringTypeConverter<ConnectionId>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct ConnectionId(string Value)
{
    public static implicit operator string(ConnectionId id) => id.Value;

    public static implicit operator ConnectionId(string value) => new(value);

    public override string ToString() => Value;
}
