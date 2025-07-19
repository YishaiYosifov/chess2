using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests;

public class AlgebraicPointTests : BaseUnitTest
{
    [Fact]
    public void AdditionOperator_adds_coordinates()
    {
        var p1 = new AlgebraicPoint(2, 3);
        var p2 = new Offset(4, 1);

        var result = p1 + p2;

        result.X.Should().Be(6);
        result.Y.Should().Be(4);
    }

    [Fact]
    public void SubtractionOperator_subtracts_coordinates()
    {
        var p1 = new AlgebraicPoint(5, 7);
        var p2 = new Offset(3, 2);

        var result = p1 - p2;

        result.X.Should().Be(2);
        result.Y.Should().Be(5);
    }

    [Theory]
    [InlineData("a1", 0, 0)]
    [InlineData("b2", 1, 1)]
    [InlineData("z8", 25, 7)]
    [InlineData("a9", 0, 8)]
    public void Constructor_parses_algebraic_string(string algebraic, int expectedX, int expectedY)
    {
        var point = new AlgebraicPoint(algebraic);
        point.X.Should().Be(expectedX);
        point.Y.Should().Be(expectedY);
    }

    [Theory]
    [InlineData(0, 0, "a1")]
    [InlineData(1, 1, "b2")]
    [InlineData(25, 7, "z8")]
    public void AsAlgebraic_returns_correct_string(int x, int y, string expected)
    {
        var point = new AlgebraicPoint(x, y);

        var result = point.AsAlgebraic();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 10, 0)]
    [InlineData(1, 1, 10, 11)]
    [InlineData(7, 7, 10, 77)]
    [InlineData(0, 8, 10, 80)]
    [InlineData(9, 0, 10, 9)]
    [InlineData(9, 9, 10, 99)]
    [InlineData(5, 5, 10, 55)]
    [InlineData(3, 2, 10, 23)]
    public void AsIndex_calculates_correct_index(int x, int y, int boardWidth, byte expectedIndex)
    {
        var point = new AlgebraicPoint(x, y);
        var result = point.AsIndex(boardWidth);
        result.Should().Be(expectedIndex);
    }
}
