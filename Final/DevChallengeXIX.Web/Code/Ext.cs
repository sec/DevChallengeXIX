using SixLabors.ImageSharp.PixelFormats;

namespace DevChallengeXIX.Web.Code;

public static class Ext
{
    public static bool IsWhite(this Rgb24 c) => c.R == 255 && c.G == 255 && c.B == 255;

    public static int CalcDarkness(this Rgb24 c)
    {
        var dist = 1m - ((decimal) c.R / byte.MaxValue);
        var darkness = (int) Math.Round(dist * 100m);

        return darkness;
    }
}