using System.Text.Json;
using Xunit.Sdk;

namespace AnarchyChess.Api.TestInfrastructure.Serializers;

public class JsonXUnitSerializer<T> : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue) =>
        JsonSerializer.Deserialize(serializedValue, type)
        ?? throw new InvalidOperationException("Deserialization returned null.");

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
