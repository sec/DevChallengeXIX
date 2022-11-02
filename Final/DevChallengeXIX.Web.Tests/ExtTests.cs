using DevChallengeXIX.Web.Code;
using SixLabors.ImageSharp.PixelFormats;

namespace DevChallengeXIX.Web.Tests;

public class ExtTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(10, false)]
    [InlineData(255, true)]
    public void Ext_IsWhite_Should_Return_Correct_Data(byte pixel, bool expectedResult)
    {
        var sut = new Rgb24(pixel, pixel, pixel);

        Assert.Equal(expectedResult, sut.IsWhite());
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(255, 0)]
    [InlineData(255 / 2, 50)]
    [InlineData(255 / 10, 90)]
    [InlineData(255 - 255 / 10, 10)]
    public void Ext_CalcDarkness_Should_Be_Correct(byte pixel, int expected)
    {
        var rgb = new Rgb24(pixel, pixel, pixel);
        var result = rgb.CalcDarkness();

        Assert.Equal(expected, result);
    }
}