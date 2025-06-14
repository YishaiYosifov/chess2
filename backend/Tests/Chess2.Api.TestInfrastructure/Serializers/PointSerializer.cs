using Chess2.Api.GameLogic.Models;
using System.Text.Json;
using Xunit.Sdk;

namespace Chess2.Api.TestInfrastructure.Serializers;

public class PointSerializer : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue) =>
        JsonSerializer.Deserialize(serializedValue, type)
        ?? throw new InvalidOperationException("Deserialization returned null.");

    public bool IsSerializable(Type type, object? value, out string failureReason)
    {
        if (type == typeof(Point) || typeof(IEnumerable<Point>).IsAssignableFrom(type))
        {
            failureReason = "";
            return true;
        }
        failureReason = $"Type {type.Name} is not supported by {nameof(PointSerializer)}.";
        return false;
    }

    public string Serialize(object value) => JsonSerializer.Serialize(value);
}
