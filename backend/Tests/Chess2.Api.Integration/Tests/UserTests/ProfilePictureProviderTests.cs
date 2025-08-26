using Chess2.Api.TestInfrastructure;
using Chess2.Api.Users.Errors;
using Chess2.Api.Users.Models;
using Chess2.Api.Users.Services;
using FluentAssertions;
using FluentStorage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Chess2.Api.Integration.Tests.UserTests;

public class ProfilePictureProviderTests : BaseIntegrationTest
{
    private readonly IProfilePictureProvider _profilePictureProvider;
    private readonly IBlobStorage _storage;
    private readonly UserId _userId;

    public ProfilePictureProviderTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _profilePictureProvider =
            Scope.ServiceProvider.GetRequiredService<IProfilePictureProvider>();
        _storage = Scope.ServiceProvider.GetRequiredService<IBlobStorage>();

        _userId = Guid.NewGuid().ToString();
    }

    private static MemoryStream CreateTestImage(
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

    private static string GetPfpPath(UserId userId) => $"profile-pictures/{userId}";

    [Fact]
    public async Task UploadProfilePictureAsync_processes_image_correctly()
    {
        int width = 30;
        int height = 20;

        var stream = CreateTestImage(width, height, color: SKColors.White);
        await _profilePictureProvider.UploadProfilePictureAsync(_userId, stream, CT);

        var bytes = await _storage.ReadBytesAsync(GetPfpPath(_userId), CT);
        using var bitmap = SKBitmap.Decode(bytes);

        int squareSize = 30;
        bitmap.Width.Should().Be(squareSize);
        bitmap.Height.Should().Be(squareSize);
        int padding = (squareSize - height) / 2;

        // bottom padding
        for (int y = 0; y < padding; y++)
        for (int x = 0; x < squareSize; x++)
            bitmap.GetPixel(x, y).Alpha.Should().Be(0);

        // top padding
        for (int y = squareSize - padding; y < squareSize; y++)
        for (int x = 0; x < squareSize; x++)
            bitmap.GetPixel(x, y).Alpha.Should().Be(0);

        // center area should be our image
        for (int y = padding; y < squareSize - padding; y++)
        {
            for (int x = 0; x < squareSize; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                pixel.Should().Be(SKColors.White);
            }
        }
    }

    [Fact]
    public async Task UploadProfilePictureAsync_crops_image_if_its_above_max_dimensions()
    {
        var stream = CreateTestImage(ProfilePictureProvider.MaxDimensionPx + 100, 10);
        await _profilePictureProvider.UploadProfilePictureAsync(_userId, stream, CT);

        var bytes = await _storage.ReadBytesAsync(GetPfpPath(_userId), CT);
        using var bitmap = SKBitmap.Decode(bytes);

        bitmap.Width.Should().Be(ProfilePictureProvider.MaxDimensionPx);
        bitmap.Height.Should().Be(ProfilePictureProvider.MaxDimensionPx);
    }

    [Fact]
    public async Task DeleteProfilePictureAsync_removes_uploaded_picture()
    {
        var stream = CreateTestImage();

        await _profilePictureProvider.UploadProfilePictureAsync(_userId, stream, CT);
        await _profilePictureProvider.DeleteProfilePictureAsync(_userId, CT);

        var exists = await _storage.ExistsAsync(GetPfpPath(_userId), CT);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetProfilePictureAsync_returns_default_when_no_picture_uploaded()
    {
        var expectedDefault = await File.ReadAllBytesAsync(
            Path.Combine(AppContext.BaseDirectory, "Data", "defaultProfilePicture.webp"),
            CT
        );

        var bytes = await _profilePictureProvider.GetProfilePictureAsync(_userId, CT);

        bytes.Should().BeEquivalentTo(expectedDefault);
    }

    [Fact]
    public async Task GetProfilePictureAsync_returns_uploaded_picture_of_the_correct_user()
    {
        UserId userId2 = Guid.NewGuid().ToString();

        var user1Stream = CreateTestImage(color: SKColors.White);
        var user2Stream = CreateTestImage(color: SKColors.Black);

        await _profilePictureProvider.UploadProfilePictureAsync(_userId, user1Stream, CT);
        await _profilePictureProvider.UploadProfilePictureAsync(userId2, user2Stream, CT);

        var user1Image = SKBitmap.Decode(
            await _profilePictureProvider.GetProfilePictureAsync(_userId, CT)
        );
        var user2Image = SKBitmap.Decode(
            await _profilePictureProvider.GetProfilePictureAsync(userId2, CT)
        );

        user1Image.GetPixel(0, 0).Should().Be(SKColors.White);
        user2Image.GetPixel(0, 0).Should().Be(SKColors.Black);
    }

    [Fact]
    public async Task GetLastModifiedAsync_returns_value_after_upload()
    {
        var stream = CreateTestImage();

        await _profilePictureProvider.UploadProfilePictureAsync(_userId, stream, CT);

        var lastModified = await _profilePictureProvider.GetLastModifiedAsync(_userId, CT);
        lastModified.Should().NotBe(DateTimeOffset.MinValue);
        lastModified.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task GetLastModifiedAsync_returns_MinValue_if_no_picture_uploaded()
    {
        var lastModified = await _profilePictureProvider.GetLastModifiedAsync(_userId, CT);

        lastModified.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_returns_error_for_invalid_file()
    {
        await using MemoryStream ms = new([1, 2, 3, 4, 5]);

        var result = await _profilePictureProvider.UploadProfilePictureAsync(_userId, ms, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.InvalidProfilePicture);
    }
}
