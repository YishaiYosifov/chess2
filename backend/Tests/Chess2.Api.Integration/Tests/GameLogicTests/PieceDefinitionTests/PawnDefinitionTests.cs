using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class PawnDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(PawnDefinitionTestData))]
    public void PawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class PawnDefinitionTestData : PawnLikeTestData
{
    public PawnDefinitionTestData()
    {
        AddRegularMoveTests(PieceType.Pawn, maxInitialMoveDistance: 3);
        AddPromotionTests(PieceType.Pawn);
    }
}
