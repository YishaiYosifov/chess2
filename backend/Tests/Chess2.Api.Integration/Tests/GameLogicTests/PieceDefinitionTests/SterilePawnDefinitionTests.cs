using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class SterilePawnDefinitionTests(Chess2WebApplicationFactory factory)
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
