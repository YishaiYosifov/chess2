using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class AntiqueenDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(HorseyDefinitionTestData))]
    public void AntiqueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class AntiqueenDefinitionTestData : KnightLikeTestData
{
    public AntiqueenDefinitionTestData()
    {
        AddKnightLikeMoves(PieceFactory.White(PieceType.Antiqueen));
    }
}
