using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class ChildPawnDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(PawnDefinitionTestData))]
    public void ChildPawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class ChildPawnDefinitionTestData : BasePawnDefinitionTestData
{
    public ChildPawnDefinitionTestData()
    {
        AddTestCases(PieceType.ChildPawn, 2);
    }
}
