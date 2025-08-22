using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class PieceToLetterTests : BaseUnitTest
{
    private readonly PieceToLetter _pieceToLetter = new();

    public static TheoryData<PieceType, string> PieceTypeTestData() =>
        new()
        {
            { PieceType.King, "k" },
            { PieceType.Queen, "q" },
            { PieceType.Pawn, "p" },
            { PieceType.UnderagePawn, "d" },
            { PieceType.Rook, "r" },
            { PieceType.Bishop, "b" },
            { PieceType.Horsey, "h" },
            { PieceType.Knook, "n" },
            { PieceType.Antiqueen, "a" },
        };

    [Theory]
    [MemberData(nameof(PieceTypeTestData))]
    public void GetLetter_returns_the_correct_letter_for_PieceType(
        PieceType piece,
        string expectedLetter
    )
    {
        var result = _pieceToLetter.GetLetter(piece);

        result.Should().Be(expectedLetter);
    }

    [Fact]
    public void AllPieceTypes_should_be_tested()
    {
        var testedPieces = PieceTypeTestData().Select(x => x.Data.Item1).ToHashSet();
        var allPieces = Enum.GetValues<PieceType>().ToHashSet();

        allPieces
            .Should()
            .BeSubsetOf(
                testedPieces,
                because: $"All PieceType enum values must be covered by the test data. Missing: {string.Join(", ", allPieces.Except(testedPieces))}"
            );
    }

    [Fact]
    public void GetLetter_returns_a_question_mark_for_unknown_pieces()
    {
        // some invalid PieceType
        var unknownPiece = (PieceType)999;

        var result = _pieceToLetter.GetLetter(unknownPiece);

        result.Should().Be("?");
    }
}
