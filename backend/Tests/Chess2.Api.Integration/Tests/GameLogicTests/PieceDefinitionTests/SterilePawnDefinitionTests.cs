using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
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
    public SterilePawnDefinitionTestData()
    {
        AddRegularMoveTests(PieceType.SterilePawn, maxInitialMoveDistance: 1);

        var movedWhitePawn = PieceFactory.White(PieceType.SterilePawn, timesMoved: 1);
        var movedBlackPawn = PieceFactory.Black(PieceType.SterilePawn, timesMoved: 1);

        Add(
            PieceTestCase
                .From("f9", movedWhitePawn)
                .GoesTo("f10")
                .WithDescription("Sterile white pawn can't promote")
        );

        Add(
            PieceTestCase
                .From("f2", movedBlackPawn)
                .GoesTo("f1")
                .WithDescription("Sterile black pawn can't promote")
        );
    }
}
