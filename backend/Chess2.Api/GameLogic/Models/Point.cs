namespace Chess2.Api.GameLogic.Models;

public readonly record struct Point(int X, int Y)
{
    public static Point operator +(Point point1, Point point2) =>
        new(point1.X + point2.X, point1.Y + point2.Y);

    public static Point operator -(Point point1, Point point2) =>
        new(point1.X - point2.X, point1.Y - point2.Y);

    public string AsAlgebraic()
    {
        var rank = (char)('a' + X);
        return $"{rank}{Y + 1}";
    }

    public override string ToString() => $"({X}, {Y})";
}

public readonly record struct AlgebraicPoint
{
    public Point Point { get; }

    public AlgebraicPoint(string algebraic)
    {
        var rank = algebraic[0] - 'a';
        var file = int.Parse(algebraic[1..]) - 1;
        Point = new Point(rank, file);
    }

    public override string ToString() => Point.AsAlgebraic();

    public static implicit operator Point(AlgebraicPoint a) => a.Point;
}
