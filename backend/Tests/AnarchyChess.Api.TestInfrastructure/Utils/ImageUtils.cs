using Refit;
using SkiaSharp;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public static class ImageUtils
{
    public static MemoryStream CreateTestImageStream(
        int width = 10,
        int height = 10,
        SKColor? color = null
    )
    {
        using SKBitmap bitmap = new(width, height);

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color ?? SKColors.White);

        MemoryStream ms = new();
        using var image = SKImage.FromBitmap(bitmap);
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
        ms.Position = 0;

        return ms;
    }

    public static StreamPart CreateTestFormImage(
        int width = 10,
        int height = 10,
        SKColor? color = null
    )
    {
        var stream = CreateTestImageStream(width, height, color);
        return new StreamPart(stream, fileName: "test.png", contentType: "image/png");
    }
}
