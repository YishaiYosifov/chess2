using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using ErrorOr;
using FluentStorage.Blobs;
using SkiaSharp;

namespace Chess2.Api.Profile.Services;

public interface IProfilePictureProvider
{
    Task DeleteProfilePictureAsync(UserId userId, CancellationToken token = default);
    Task<DateTimeOffset> GetLastModifiedAsync(UserId userId, CancellationToken token = default);
    Task<byte[]> GetProfilePictureAsync(UserId userId, CancellationToken token = default);
    Task<ErrorOr<Created>> UploadProfilePictureAsync(
        UserId userId,
        Stream fileStream,
        CancellationToken token = default
    );
}

public class ProfilePictureProvider : IProfilePictureProvider
{
    private readonly IBlobStorage _storage;
    private readonly byte[] _defaultProfilePicture;

    public const int MaxDimensionPx = 512;

    public ProfilePictureProvider(IBlobStorage storage)
    {
        _storage = storage;

        var defaultPfpPath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "defaultProfilePicture.webp"
        );
        _defaultProfilePicture = File.ReadAllBytes(defaultPfpPath);
    }

    private static string GetPfpPath(UserId userId) => $"profile-pictures/{userId}";

    public async Task<ErrorOr<Created>> UploadProfilePictureAsync(
        UserId userId,
        Stream fileStream,
        CancellationToken token = default
    )
    {
        SKBitmap original;
        try
        {
            original = SKBitmap.Decode(fileStream);
            if (original is null)
                return UserErrors.InvalidProfilePicture;
        }
        catch
        {
            return UserErrors.InvalidProfilePicture;
        }

        int width = original.Width;
        int height = original.Height;

        // resize to fit in MaxDimensionPx
        float scale = Math.Min(1, (float)MaxDimensionPx / Math.Max(width, height));
        int scaledWidth = (int)(width * scale);
        int scaledHeight = (int)(height * scale);
        using var resized =
            original.Resize(new SKImageInfo(scaledWidth, scaledHeight), SKSamplingOptions.Default)
            ?? original;

        // make the image a square that is <= MaxDimensionPx
        int squareSize = Math.Min(MaxDimensionPx, Math.Max(scaledWidth, scaledHeight));
        using SKBitmap squareBitmap = new(squareSize, squareSize);
        using SKCanvas canvas = new(squareBitmap);
        canvas.Clear(SKColors.Transparent);

        int offsetX = (squareSize - resized.Width) / 2;
        int offsetY = (squareSize - resized.Height) / 2;
        canvas.DrawBitmap(resized, offsetX, offsetY);
        canvas.Flush();

        using var image = SKImage.FromBitmap(squareBitmap);
        using MemoryStream ms = new();
        image.Encode(SKEncodedImageFormat.Webp, 80).SaveTo(ms);
        ms.Position = 0;

        await _storage.WriteAsync(GetPfpPath(userId), ms, cancellationToken: token);
        return Result.Created;
    }

    public Task DeleteProfilePictureAsync(UserId userId, CancellationToken token = default) =>
        _storage.DeleteAsync(GetPfpPath(userId), token);

    public async Task<byte[]> GetProfilePictureAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        if (!await _storage.ExistsAsync(GetPfpPath(userId), token))
            return _defaultProfilePicture;

        var bytes = await _storage.ReadBytesAsync(GetPfpPath(userId), token);
        return bytes is null || bytes.Length == 0 ? _defaultProfilePicture : bytes;
    }

    public async Task<DateTimeOffset> GetLastModifiedAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        if (!await _storage.ExistsAsync(GetPfpPath(userId), token))
            return DateTimeOffset.MinValue;

        var blob = await _storage.GetBlobAsync(GetPfpPath(userId), token);
        return blob.LastModificationTime ?? DateTimeOffset.MinValue;
    }
}
