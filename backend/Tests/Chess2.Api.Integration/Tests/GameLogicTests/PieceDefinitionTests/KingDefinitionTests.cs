using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class KingDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(KingDefinitionTestData))]
    public void KingDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class KingDefinitionTestData : TheoryData<PieceTestCase>
{
    public KingDefinitionTestData()
    {
        var king = PieceFactory.White(PieceType.King);
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

        Add(
            PieceTestCase
                .From("d4", king)
                .GoesTo("d5") // up
                .GoesTo("e5") // up-right
                .GoesTo("e4") // right
                .GoesTo("e3") // down-right
                .GoesTo("d3") // down
                .GoesTo("c3") // down-left
                .GoesTo("c4") // left
                .GoesTo("c5") // up-left
                .WithDescription("Open board from d4")
        );

        Add(
            PieceTestCase
                .From("d4", king)
                .WithPieceAt("d5", friend)
                .WithPieceAt("e5", friend)
                .WithPieceAt("e4", friend)
                .WithPieceAt("e3", friend)
                .WithPieceAt("d3", friend)
                .WithPieceAt("c3", friend)
                .WithPieceAt("c4", friend)
                .WithPieceAt("c5", friend)
                .WithDescription("Surrounded by friendly pieces - no moves")
        );

        Add(
            PieceTestCase
                .From("d4", king)
                .WithPieceAt("d5", enemy)
                .WithPieceAt("e5", enemy)
                .WithPieceAt("e4", enemy)
                .WithPieceAt("e3", enemy)
                .WithPieceAt("d3", enemy)
                .WithPieceAt("c3", enemy)
                .WithPieceAt("c4", enemy)
                .WithPieceAt("c5", enemy)
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("e5", captures: ["e5"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("e3", captures: ["e3"])
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("c3", captures: ["c3"])
                .GoesTo("c4", captures: ["c4"])
                .GoesTo("c5", captures: ["c5"])
                .WithDescription("Surrounded by enemies - all moves are captures")
        );

        Add(
            PieceTestCase
                .From("a1", king)
                .GoesTo("a2") // up
                .GoesTo("b2") // up-right
                .GoesTo("b1") // right
                .WithDescription("Edge of board: king on a1")
        );

        Add(
            PieceTestCase
                .From("j10", king)
                .GoesTo("i10") // left
                .GoesTo("i9") // down-left
                .GoesTo("j9") // down
                .WithDescription("Corner case: king on j10 (top-right corner)")
        );

        Add(
            PieceTestCase
                .From("h1", king)
                .WithPieceAt("i2", friend)
                .WithPieceAt("g2", enemy)
                .GoesTo("h2") // up
                // i2 blocked
                .GoesTo("i1") // right
                .GoesTo("g1") // left
                .GoesTo("g2", captures: ["g2"]) // up-left capture
                .WithDescription("King on h1, friend at i2, enemy at g2")
        );

        var rook = PieceFactory.White(PieceType.Rook);
        Move rookKingsideCastle = new(new("j1"), new("g1"), rook);
        Move rookQueensideCastle = new(new("a1"), new("e1"), rook);
        Add(
            PieceTestCase
                .From("f1", king)
                .WithPieceAt("j1", rook) // Kingside rook
                .WithPieceAt("a1", rook) // Queenside rook
                .GoesTo(
                    "h1",
                    trigger: ["i1"],
                    sideEffects: [rookKingsideCastle],
                    specialMoveType: SpecialMoveType.KingsideCastle
                )
                .GoesTo(
                    "d1",
                    trigger: ["c1", "b1"],
                    sideEffects: [rookQueensideCastle],
                    specialMoveType: SpecialMoveType.QueensideCastle
                )
                // regular moves
                .GoesTo("e1")
                .GoesTo("e2")
                .GoesTo("f2")
                .GoesTo("g2")
                .GoesTo("g1")
                .WithDescription("King on f1 with rooks in castling position")
        );
    }
}
