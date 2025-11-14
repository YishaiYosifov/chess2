using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class SterilePawnDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(SterilePawnDefinitionTestData))]
    public void SterilePawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class SterilePawnDefinitionTestData : PawnLikeTestData
{
    private static readonly IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
    ];

    public SterilePawnDefinitionTestData()
    {
        AddMoveTests(PieceType.SterilePawn, maxInitialMoveDistance: 1, promotesTo: _promotesTo);
    }
}
