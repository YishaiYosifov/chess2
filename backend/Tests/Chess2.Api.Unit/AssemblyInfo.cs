using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Serializers;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(
    typeof(JsonXUnitSerializer<AlgebraicPoint>),
    typeof(AlgebraicPoint),
    typeof(IEnumerable<AlgebraicPoint>)
)]

[assembly: RegisterXunitSerializer(
    typeof(JsonXUnitSerializer<Offset>),
    typeof(Offset),
    typeof(IEnumerable<Offset>)
)]
