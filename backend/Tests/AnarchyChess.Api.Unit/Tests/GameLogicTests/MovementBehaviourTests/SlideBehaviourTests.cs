using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class SlideBehaviourTests
{
    [Theory]
    [ClassData(typeof(SlideBehaviourTestData))]
    public void SlideBehaviour_evaluates_expected_position(
        AlgebraicPoint from,
        Offset offset,
        IEnumerable<AlgebraicPoint> expectedPoints,
        IEnumerable<AlgebraicPoint> blockingPieces
    )
    {
        var piece = PieceFactory.White();
        var board = new ChessBoard([]);
        board.PlacePiece(from, piece);
        foreach (var p in blockingPieces ?? [])
            board.PlacePiece(p, PieceFactory.Black());

        var behaviour = new SlideBehaviour(offset);

        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }

    [Theory]
    [ClassData(typeof(SlideBehaviourWithMaxTestData))]
    public void SlideBehaviour_respects_max_distance(
        AlgebraicPoint from,
        Offset offset,
        int max,
        IEnumerable<AlgebraicPoint> expectedPoints
    )
    {
        var piece = PieceFactory.White();
        var board = new ChessBoard([]);
        board.PlacePiece(from, piece);

        var behaviour = new SlideBehaviour(offset, max);

        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }
}

public class SlideBehaviourTestData
    : TheoryData<
        AlgebraicPoint, // from position
        Offset, // offset to slide
        IEnumerable<AlgebraicPoint>, // expected points to slide to
        IEnumerable<AlgebraicPoint> // blocking pieces (if any)
    >
{
    public SlideBehaviourTestData()
    {
        // Slide horizontally to the right from e4
        Add(new("e4"), new(1, 0), [new("f4"), new("g4"), new("h4"), new("i4"), new("j4")], []);

        // Slide vertically upward from e4
        Add(
            new("e4"),
            new(0, 1),
            [new("e5"), new("e6"), new("e7"), new("e8"), new("e9"), new("e10")],
            []
        );

        // Diagonal up-right from e5
        Add(new("e5"), new(-1, 1), [new("d6"), new("c7"), new("b8"), new("a9")], []);

        // Horizontal left from e4
        Add(new("e4"), new(-1, 0), [new("d4"), new("c4"), new("b4"), new("a4")], []);

        // Diagonal down-right from f6
        Add(new("f6"), new(1, 1), [new("g7"), new("h8"), new("i9"), new("j10")], []);

        // Diagonal up-left from g7
        Add(
            new("g7"),
            new(-1, -1),
            [new("f6"), new("e5"), new("d4"), new("c3"), new("b2"), new("a1")],
            []
        );

        // Long down-right from b2
        Add(
            new("b2"),
            new(1, 1),
            [
                new("c3"),
                new("d4"),
                new("e5"),
                new("f6"),
                new("g7"),
                new("h8"),
                new("i9"),
                new("j10"),
            ],
            []
        );

        // Downward from f6
        Add(new("f6"), new(0, -1), [new("f5"), new("f4"), new("f3"), new("f2"), new("f1")], []);

        // Edge cases
        Add(new("j10"), new(1, 0), [], []);
        Add(new("a1"), new(-1, 0), [], []);
        Add(new("f10"), new(0, 1), [], []);
        Add(new("f1"), new(0, -1), [], []);

        // Blocked horizontally (g4 blocks after g4)
        Add(new("e4"), new(1, 0), [new("f4"), new("g4")], [new("g4")]);

        // Blocked diagonally (c7 blocks after c7)
        Add(new("e5"), new(-1, 1), [new("d6"), new("c7")], [new("c7")]);

        // Blocked down-right (h8 blocks)
        Add(new("f6"), new(1, 1), [new("g7"), new("h8")], [new("h8")]);

        // Blocked upward (d5 blocks)
        Add(new("d4"), new(0, 1), [new("d5")], [new("d5")]);

        // Blocked left (a4 blocks)
        Add(new("d4"), new(-1, 0), [new("c4"), new("b4"), new("a4")], [new("a4")]);
    }
}

public class SlideBehaviourWithMaxTestData
    : TheoryData<
        AlgebraicPoint, // from position
        Offset, // offset to slide
        int, // max distance
        IEnumerable<AlgebraicPoint> // expected positions
    >
{
    public SlideBehaviourWithMaxTestData()
    {
        // max = 1
        Add(new("e4"), new(1, 0), 1, [new("f4")]);

        // max = 2
        Add(new("e4"), new(0, 1), 2, [new("e5"), new("e6")]);

        // max = 3
        Add(new("a1"), new(1, 1), 3, [new("b2"), new("c3"), new("d4")]);

        // max = 0 (should yield nothing)
        Add(new("e4"), new(1, 0), 0, []);
    }
}
