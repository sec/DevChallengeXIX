using DevChallengeXIX.Web.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DevChallengeXIX.Web.Tests;

public class ImageRequestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ImageRequestTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData(0, ImageSamples.Example1, 19, 39, 20 * 40)]
    [InlineData(100, ImageSamples.Example1, 0, 0, 0)]
    [InlineData(70, ImageSamples.Example1, 11, 39, 110)]
    [InlineData(50, ImageSamples.Example2, 11, 11, 141)]
    [InlineData(49, ImageSamples.Example2, 11, 11, 144)]
    [InlineData(0, ImageSamples.Example2, 11, 11, 12 * 12)]
    [InlineData(100, ImageSamples.Example2, 0, 0, 0)]
    [InlineData(94, ImageSamples.Example1, 10, 39, 77)]
    [InlineData(90, ImageSamples.Example1, 10, 39, 90)]
    [InlineData(0, ImageSamples.SingleHalf10by10, 0, 0, 1)]
    [InlineData(25, ImageSamples.SingleHalf10by10, 0, 0, 1)]
    [InlineData(75, ImageSamples.SingleHalf10by10, 0, 0, 1)]
    [InlineData(76, ImageSamples.SingleHalf10by10, 0, 0, 0)]
    [InlineData(100, ImageSamples.SingleBlack10by10, 0, 0, 1)]
    [InlineData(0, ImageSamples.SingleBlack10by10, 0, 0, 1)]
    [InlineData(0, ImageSamples.SingleWhite10by10, 0, 0, 1)]
    [InlineData(1, ImageSamples.BigAllBlack, 9, 9, 100, 1000, 1000)]
    [InlineData(1, ImageSamples.Black15by5, 0, 0, 1)]
    public async void ImageRequest_Should_Process_Images_Correctly(int minLevel, string image, int expectedMaxX, int expectedMaxY, int expectedMines, int gridX = 0, int gridY = 0)
    {
        var request = new ImageRequest(minLevel, image, gridX, gridY);
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(Program.ImageInputEndpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ImageRequestResponse>();
        Assert.NotNull(data);

        var mines = data!.Mines.ToList();
        Assert.Equal(expectedMines, mines.Count);

        if (expectedMines > 0)
        {
            Assert.Equal(expectedMaxX, mines.MaxBy(x => x.X)!.X);
            Assert.Equal(expectedMaxY, mines.MaxBy(x => x.Y)!.Y);
        }
    }

    [Fact]
    public async void ImageRequest_Should_Return_Error_On_Invalid_Image()
    {
        var request = new ImageRequest(0, Guid.NewGuid().ToString());
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(Program.ImageInputEndpoint, request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<ImageErrorReposne>();
        Assert.NotNull(data);
        Assert.Contains("image", data!.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async void ImageRequest_Should_Return_Width_Error_On_All_White_Image(int width)
    {
        var request = new ImageRequest(0, ImageSamples.AllWhite, width);
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(Program.ImageInputEndpoint, request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<ImageErrorReposne>();
        Assert.NotNull(data);
        Assert.Contains("width", data!.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async void ImageRequest_Should_Return_Height_Error_On_All_White_Image(int height)
    {
        var request = new ImageRequest(0, ImageSamples.AllWhite, 10, height);
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(Program.ImageInputEndpoint, request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<ImageErrorReposne>();
        Assert.NotNull(data);
        Assert.Contains("height", data!.Error);
    }

    [Fact]
    public async void ImageRequest_Should_Work_On_All_White_Image_Given_Width_And_Height()
    {
        var request = new ImageRequest(0, ImageSamples.AllWhite, 1, 1);
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(Program.ImageInputEndpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<ImageRequestResponse>();
        Assert.NotNull(data);
    }
}