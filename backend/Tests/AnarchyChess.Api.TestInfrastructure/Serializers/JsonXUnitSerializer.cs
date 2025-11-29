using System.Text.Json;
using Xunit.Sdk;

namespace AnarchyChess.Api.TestInfrastructure.Serializers;

public class JsonXUnitSerializer<T> : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue)
    {
        if (type == typeof(T))
            return JsonSerializer.Deserialize<T>(serializedValue)
                ?? throw new InvalidOperationException("Deserialization returned null.");

        if (typeof(IEnumerable<T>).IsAssignableFrom(type))
        {
            var list =
                JsonSerializer.Deserialize<List<T>>(serializedValue)
                ?? throw new InvalidOperationException("Deserialization returned null.");
            return list;
        }

        throw new InvalidOperationException(
            $"Type {type} is not supported by {nameof(JsonXUnitSerializer<T>)}."
        );
    }

    public bool IsSerializable(Type type, object? value, out string failureReason)
    {
        if (type == typeof(T) || typeof(IEnumerable<T>).IsAssignableFrom(type))
        {
            failureReason = "";
            return true;
        }
        failureReason = $"Type {type.Name} is not supported by {nameof(JsonXUnitSerializer<T>)}.";
        return false;
    }

    public string Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }
}
