namespace Chess2.Api.GameLogic.Models;

public readonly record struct AlgebraicPoint(int X, int Y)
{
    public AlgebraicPoint(string algebraic)
        : this(algebraic[0] - 'a', int.Parse(algebraic[1..]) - 1) { }

    public static AlgebraicPoint operator +(AlgebraicPoint left, Offset right) =>
        new(left.X + right.X, left.Y + right.Y);

    public static AlgebraicPoint operator -(AlgebraicPoint left, Offset right) =>
        new(left.X - right.X, left.Y - right.Y);

    public string AsAlgebraic()
    {
        var rank = (char)('a' + X);
        return $"{rank}{Y + 1}";
    }

    public override string ToString() => AsAlgebraic();
}
