using System.Net;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;
using FluentAssertions;
using FluentStorage.Utils.Extensions;
using Refit;
using SkiaSharp;

namespace AnarchyChess.Api.Functional.Tests.ProfileControllerTests;

public class ProfilePictureTests(AnarchyChessWebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task UploadProfilePicture_succeeds_and_can_be_retrieved()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var file = ImageUtils.CreateTestFormImage(color: SKColors.Black);
        var uploadResponse = await ApiClient.Api.UploadProfilePictureAsync(file);

        uploadResponse.IsSuccessful.Should().BeTrue();

        var getResponse = await ApiClient.Api.GetProfilePictureAsync(user.Id);
        getResponse.IsSuccessful.Should().BeTrue();
        getResponse.Content.Should().NotBeNull();

        using var bitmap = SKBitmap.Decode(getResponse.Content.ToByteArray());
        bitmap.GetPixel(0, 0).Should().Be(SKColors.Black);
    }

    [Fact]
    public async Task DeleteProfilePicture_removes_uploaded_picture()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var file = ImageUtils.CreateTestFormImage();
        await ApiClient.Api.UploadProfilePictureAsync(file);

        var deleteResponse = await ApiClient.Api.DeleteProfilePictureAsync();
        deleteResponse.IsSuccessful.Should().BeTrue();

        var getResponse = await ApiClient.Api.GetProfilePictureAsync(user.Id);
        getResponse.IsSuccessful.Should().BeTrue();

        getResponse.Headers.ETag?.Tag.Should().Be("\"0\"");
    }

    [Fact]
    public async Task GetProfilePicture_returns_304_if_etag_matches()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var file = ImageUtils.CreateTestFormImage(color: SKColors.Green);
        await ApiClient.Api.UploadProfilePictureAsync(file);

        var firstResponse = await ApiClient.Api.GetProfilePictureAsync(user.Id);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var etag = firstResponse.Headers.ETag?.Tag;
        etag.Should().NotBeNull();
        etag.Should().NotBe("\"0\"");

        var secondResponse = await ApiClient.Api.GetProfilePictureAsync(user.Id, etag);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task UploadProfilePicture_rejects_invalid_file()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        using var ms = new MemoryStream([1, 2, 3, 4, 5]);
        var file = new StreamPart(ms, "pfp.png", "image/png");

        var uploadResponse = await ApiClient.Api.UploadProfilePictureAsync(file);

        uploadResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
