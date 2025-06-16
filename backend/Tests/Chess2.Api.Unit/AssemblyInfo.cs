using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Serializers;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(
    typeof(JsonXUnitSerializer<Point>),
    typeof(Point),
    typeof(IEnumerable<Point>)
)]
[assembly: RegisterXunitSerializer(
    typeof(JsonXUnitSerializer<AlgebraicPoint>),
    typeof(AlgebraicPoint),
    typeof(IEnumerable<AlgebraicPoint>)
)]
