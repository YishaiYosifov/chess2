using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class UnderagePawnDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(PawnDefinitionTestData))]
    public void UnderagePawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class UnderagePawnDefinitionTestData : BasePawnDefinitionTestData
{
    public UnderagePawnDefinitionTestData()
    {
        AddTestCases(PieceType.UnderagePawn, 2);
    }
}
