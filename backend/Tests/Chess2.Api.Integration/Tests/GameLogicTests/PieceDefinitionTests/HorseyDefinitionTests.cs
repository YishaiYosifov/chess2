using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class HorseyDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(HorseyDefinitionTestData))]
    public void HorseyDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class HorseyDefinitionTestData : KnightLikeTestData
{
    private static readonly Piece _horsey = PieceFactory.White(PieceType.Horsey);

    public HorseyDefinitionTestData()
        : base(_horsey)
    {
        Add(
            PieceTestCase
                .From("e5", _horsey)
                .WithWhitePieceAt("d7", PieceType.Rook)
                .WithEnemyPieceAt("c8")
                .WithFriendlyPieceAt("d8")
                .WithEnemyPieceAt("e8")
                .WithFriendlyPieceAt("c7")
                .WithEnemyPieceAt("e7")
                .WithFriendlyPieceAt("c6")
                .WithEnemyPieceAt("d6")
                .WithFriendlyPieceAt("e6")
                .GoesTo(
                    "d7",
                    captures: ["c8", "d8", "e8", "c7", "d7", "e7", "c6", "d6", "e6"],
                    promotesTo: PieceType.Knook,
                    specialMoveType: SpecialMoveType.KnooklearFusion
                )
                .GoesTo("f7", "g6", "c4", "g4", "d3", "f3")
                .WithDescription("Fuses with rook and explodes surrounding pieces")
        );
    }
}
