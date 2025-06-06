using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Serializers;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(PointSerializer), typeof(Point))]

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class SlideBehaviourTests
{
    [Theory]
    [ClassData(typeof(SlideBehaviourTestData))]
    public void Test(Point from, Point offset)
    {
        var chessboard = new ChessBoard(new() [
            [from] = new PieceFaker().Generate()
        ])
        var slideBehaviour = new SlideBehaviour(offset);
    }
}

public class SlideBehaviourTestData : TheoryData<Point, Point>
{
    public SlideBehaviourTestData()
    {
        Add(new Point(1, 1), new Point(2, 2));
    }
}
