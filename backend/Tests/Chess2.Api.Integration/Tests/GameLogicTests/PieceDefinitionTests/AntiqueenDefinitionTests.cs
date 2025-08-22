using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class AntiqueenDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(HorseyDefinitionTestData))]
    public void ANtiqueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
