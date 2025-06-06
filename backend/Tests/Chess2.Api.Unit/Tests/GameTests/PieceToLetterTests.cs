using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class PieceToLetterTests
{
    private readonly PieceToLetter _pieceToLetter = new();

    public static TheoryData<PieceType, string> PieceTypeTestData() =>
        new()
        {
            { PieceType.King, "k" },
            { PieceType.Queen, "q" },
            { PieceType.Pawn, "p" },
            { PieceType.Rook, "r" },
            { PieceType.Bishop, "b" },
            { PieceType.Horsey, "h" },
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
