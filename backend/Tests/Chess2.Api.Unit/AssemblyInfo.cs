using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Serializers;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(PointSerializer), typeof(Point))]
