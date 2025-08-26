using Chess2.Api.Users.Errors;
using Chess2.Api.Users.Models;
using ErrorOr;
using FluentStorage.Blobs;
using SkiaSharp;

namespace Chess2.Api.Users.Services;

public interface IProfilePictureProvider
{
    Task DeleteProfilePictureAsync(UserId userId, CancellationToken token = default);
    Task<DateTimeOffset> GetLastModifiedAsync(UserId userId, CancellationToken token = default);
    Task<byte[]> GetProfilePictureAsync(UserId userId, CancellationToken token = default);
    Task<ErrorOr<Created>> UploadProfilePictureAsync(
        UserId userId,
        IFormFile file,
        CancellationToken token = default
    );
}

public class ProfilePictureProvider : IProfilePictureProvider
{
    private readonly IBlobStorage _storage;
    private readonly byte[] _defaultProfilePicture;

    private const float MaxDimensionPx = 512;

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
        IFormFile file,
        CancellationToken token = default
    )
    {
        SKBitmap original;
        try
        {
            using var stream = file.OpenReadStream();
            original =
                SKBitmap.Decode(stream) ?? throw new InvalidOperationException("Invalid image");
        }
        catch
        {
            return UserErrors.InvalidProfilePicture;
        }

        int width = original.Width;
        int height = original.Height;
        float scale = Math.Min(MaxDimensionPx / width, MaxDimensionPx / height);

        int newWidth = (int)(width * scale);
        int newHeight = (int)(height * scale);

        using var resized = original.Resize(
            new SKImageInfo(newWidth, newHeight),
            SKSamplingOptions.Default
        );
        using var image = SKImage.FromBitmap(resized ?? original);
        using var ms = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Webp, 90).SaveTo(ms);
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
