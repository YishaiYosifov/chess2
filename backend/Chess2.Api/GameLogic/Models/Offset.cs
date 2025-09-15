namespace Chess2.Api.GameLogic.Models;

public readonly record struct Offset(int X, int Y)
{
    public static Offset operator *(Offset left, int right) => new(left.X * right, left.Y * right);
}
