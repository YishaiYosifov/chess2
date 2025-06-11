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

    public int AsIdx(int boardWidth) => Y * boardWidth + X;

    public override string ToString() => $"({X}, {Y})";
}
