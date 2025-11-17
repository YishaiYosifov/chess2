using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Serializers;
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

[assembly: RegisterXunitSerializer(
    typeof(JsonXUnitSerializer<TimeControlSettings>),
    typeof(TimeControlSettings)
)]
