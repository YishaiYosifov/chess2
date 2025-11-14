using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class PawnDefinitionTests(AnarchyChessWebApplicationFactory factory)
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
        AddMoveTests(
            PieceType.Pawn,
            maxInitialMoveDistance: 3,
            promotesTo: GameLogicConstants.PromotablePieces
        );
    }
}
