using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Serializers;
using AnarchyChess.Api.TestInfrastructure.Utils;
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
    typeof(JsonXUnitSerializer<PieceTestCase>),
    typeof(PieceTestCase)
)]
