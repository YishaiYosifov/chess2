using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class CheckerDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(CheckerDefinitionTestData))]
    public void CheckerDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class CheckerDefinitionTestData : TheoryData<PieceTestCase>
{
    public CheckerDefinitionTestData()
    {
        var checker = PieceFactory.White(PieceType.Checker);

        Add(
            PieceTestCase
                .From("e5", checker)
                // left up
                .GoesTo("d6")
                .GoesTo("c7")
                // right up
                .GoesTo("f6")
                .GoesTo("g7")
                // left down
                .GoesTo("d4")
                .GoesTo("c3")
                // right down
                .GoesTo("f4")
                .GoesTo("g3")
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", checker)
                .GoesTo("b2")
                .GoesTo("c3")
                .WithDescription("From the corner on a1")
        );

        Add(
            PieceTestCase
                .From("e5", checker)
                .WithFriendlyPieceAt("d6")
                .WithFriendlyPieceAt("f6")
                .WithFriendlyPieceAt("d4")
                .WithFriendlyPieceAt("f4")
                .GoesTo("c7")
                .GoesTo("g7")
                .GoesTo("c3")
                .GoesTo("g3")
                .WithDescription("Surrounded by friendly pieces, should jump over")
        );

        Add(
            PieceTestCase
                .From("e5", checker)
                .WithEnemyPieceAt("d6")
                .WithEnemyPieceAt("f6")
                .WithEnemyPieceAt("d4")
                .WithEnemyPieceAt("f4")
                .GoesTo("c7", captures: ["d6"])
                .GoesTo("g7", captures: ["f6"])
                .GoesTo("c3", captures: ["d4"])
                .GoesTo("g3", captures: ["f4"])
        );

        Add(
            PieceTestCase
                .From("e5", checker)
                .WithFriendlyPieceAt("d6")
                .WithEnemyPieceAt("d8")
                .GoesTo("c7")
                .GoesTo("e9", captures: ["d8"], intermediates: ["c7"])
                .GoesTo("f6", "g7", "d4", "c3", "f4", "g3")
                .WithDescription("Multi jump with a friend an and enemy")
        );

        Add(
            PieceTestCase
                .From("e5", checker)
                .WithEnemyPieceAt("d6")
                .WithEnemyPieceAt("d8")
                .WithEnemyPieceAt("f8")
                .WithEnemyPieceAt("f6")
                // chain from left up
                .GoesTo("c7", captures: ["d6"])
                .GoesTo("e9", captures: ["d6", "d8"], intermediates: ["c7"])
                .GoesTo("g7", captures: ["d6", "d8", "f8"], intermediates: ["c7", "e9"])
                // chain from right up
                .GoesTo("g7", captures: ["f6"])
                .GoesTo("e9", captures: ["f6", "f8"], intermediates: ["g7"])
                .GoesTo("c7", captures: ["f6", "f8", "d8"], intermediates: ["g7", "e9"])
                .GoesTo("d4", "c3", "f4", "g3")
                .WithDescription("Multi capture chain that results in a loop")
        );

        Add(
            PieceTestCase
                .From("f8", PieceFactory.White(PieceType.Checker))
                .GoesTo("g9")
                .GoesTo("h10", promotesTo: PieceType.King)
                .GoesTo("e9")
                .GoesTo("d10", promotesTo: PieceType.King)
                .GoesTo("e7", "d6", "g7", "h6")
                .WithDescription("White promotion to king")
        );

        Add(
            PieceTestCase
                .From("f3", PieceFactory.Black(PieceType.Checker))
                .GoesTo("g2")
                .GoesTo("h1", promotesTo: PieceType.King)
                .GoesTo("e2")
                .GoesTo("d1", promotesTo: PieceType.King)
                .GoesTo("e4", "d5", "g4", "h5")
                .WithDescription("Black promotion to king")
        );

        Add(
            PieceTestCase
                .From("f3", PieceFactory.White(PieceType.Checker))
                .GoesTo("g2", "h1", "e2", "d1", "e4", "d5", "g4", "h5")
                .WithDescription("White doesn't promote on their side")
        );

        Add(
            PieceTestCase
                .From("f8", PieceFactory.Black(PieceType.Checker))
                .GoesTo("g9", "h10", "e9", "d10", "e7", "d6", "g7", "h6")
                .WithDescription("Black doesn't promote on their side")
        );
    }
}
