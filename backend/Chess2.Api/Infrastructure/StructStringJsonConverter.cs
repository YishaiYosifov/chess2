using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chess2.Api.Infrastructure;

public class StructStringJsonConverter<T> : JsonConverter<T>
    where T : struct
{
    private static readonly ConstructorInfo Ctor =
        typeof(T).GetConstructor([typeof(string)])
        ?? throw new InvalidOperationException($"{typeof(T)} must have a string constructor");

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var str = reader.GetString();
        return (T)Ctor.Invoke([str]);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
