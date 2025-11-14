using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class UnderagePawnDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(UnderagePawnDefinitionTestData))]
    public void UnderagePawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class UnderagePawnDefinitionTestData : PawnLikeTestData
{
    public UnderagePawnDefinitionTestData()
    {
        AddMoveTests(
            PieceType.UnderagePawn,
            maxInitialMoveDistance: 2,
            promotesTo: GameLogicConstants.PromotablePieces
        );
    }
}
