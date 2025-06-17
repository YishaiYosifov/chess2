using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests;

public class PointTests : BaseUnitTest
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
    [InlineData(0, 0, "a1")]
    [InlineData(1, 1, "b2")]
    [InlineData(25, 7, "z8")]
    public void AsAlgebraic_returns_correct_string(int x, int y, string expected)
    {
        var point = new AlgebraicPoint(x, y);

        var result = point.AsAlgebraic();

        result.Should().Be(expected);
    }
}
