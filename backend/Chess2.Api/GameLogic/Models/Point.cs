namespace Chess2.Api.GameLogic.Models;

public readonly record struct Point(int X, int Y)
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyz";

    public static Point operator +(Point point1, Point point2) =>
        new(point1.X + point2.X, point1.Y + point2.Y);

    public static Point operator -(Point point1, Point point2) =>
        new(point1.X - point2.X, point1.Y - point2.Y);

    public string AsUCI()
    {
        return $"{Chars[X]}{Y + 1}";
    }
}
