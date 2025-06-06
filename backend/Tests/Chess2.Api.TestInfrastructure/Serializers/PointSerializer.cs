using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;


namespace Chess2.Api.TestInfrastructure.Serializers;

public class PointSerializer : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue)
    {
        var point = serializedValue.Split("-");
        var x = int.Parse(point[0]);
        var y = int.Parse(point[1]);
        return new Point(x, y);
    }

    public bool IsSerializable(Type type, object? value, out string failureReason)
    {
        if (type != typeof(Point))
        {
            failureReason = $"Type is not {nameof(Point)}";
            return false;
        }
        if (value is not Point)
        {
            failureReason = $"Value is not of type {nameof(Point)}";
            return false;
        }
        failureReason = "";
        return true;
    }

    public string Serialize(object value)
    {
        if (value is not Point point)
            throw new InvalidCastException("Could not cast ");
        return $"{point.X}-{point.Y}";
    }
}
