using DevChallengeXIX.Web.Dto;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevChallengeXIX.Web.Code;

public class ImageRequestProcessor
{
    static IResult ErrorReponse(string error, string? details = null) => Results.UnprocessableEntity(new ImageErrorReposne(error, details));

    static IResult ErrorWidth() => ErrorReponse("Cannot determine single cell width.");
    static IResult ErrorHeight() => ErrorReponse("Cannot determine single cell height.");
    static IResult ErrorImage(string details) => ErrorReponse("Unable to load image.", details);
    static IResult ErrorUnknown(string details) => ErrorReponse("Unexpected error.", details);

    public IResult ProcessImage(int minLevel, string image, int Width = 0, int Height = 0)
    {
        byte[] bytes;
        int cellWidth = Width;
        int cellHeight = Height;
        Image<Rgb24>? map = null;
        List<ImageMineResponse> result = new();

        try
        {
            #region Load image
            try
            {
                if (image.StartsWith("data:"))
                {
                    bytes = Convert.FromBase64String(image[(1 + image.IndexOf(','))..]);
                }
                else
                {
                    bytes = Convert.FromBase64String(image);
                }

                map = Image.Load<Rgb24>(bytes);
            }
            catch (Exception ex)
            {
                return ErrorImage(ex.ToString());
            }
            #endregion

            #region Find width of cell
            if (cellWidth <= 0)
            {
                for (int i = 0; i < map.Width; i++)
                {
                    var isWhite = map[i, 1].IsWhite();
                    if (i == 0 && !isWhite)
                    {
                        return ErrorWidth();
                    }
                    else if (i > 0 && isWhite)
                    {
                        cellWidth = i - 1;
                        break;
                    }
                }
                if (cellWidth <= 0)
                {
                    return ErrorWidth();
                }
            }
            #endregion

            #region Find height of cell
            if (cellHeight <= 0)
            {
                for (int i = 0; i < map.Height; i++)
                {
                    var isWhite = map[i, 1].IsWhite();
                    if (i == 0 && !isWhite)
                    {
                        return ErrorHeight();
                    }
                    else if (i > 0 && isWhite)
                    {
                        cellHeight = i - 1;
                        break;
                    }
                }
                if (cellHeight <= 0)
                {
                    return ErrorHeight();
                }
            }
            #endregion

            #region Process logic here, cut big picute into smaller ones and process small images (less memory needed)            
            for (int y = 1, cellY = 0; y < map.Height; y += 1 + cellHeight, cellY++)
            {
                for (int x = 1, cellX = 0; x < map.Width; x += 1 + cellWidth, cellX++)
                {
                    var width = cellWidth;
                    var height = cellHeight;

                    // if image is not perfect, adjust size and cut smaller image and still do the count
                    if (x + width >= map.Width)
                    {
                        width = map.Width - x;
                    }
                    if (y + height >= map.Height)
                    {
                        height = map.Height - y;
                    }

                    var slice = new Rectangle(x, y, width, height);

                    // cut small piece
                    using var small = Extract(map, slice);

                    // this could be done using tasks on multi-core cpu, but when heavy load, we can process multiple big images, so let's use single cpu per image
                    small.ProcessPixelRows(accessor =>
                    {
                        var dark = new List<int>();

                        for (int y = 0; y < accessor.Height; y++)
                        {
                            var row = accessor.GetRowSpan(y);

                            for (int x = 0; x < row.Length; x++)
                            {
                                dark.Add(row[x].CalcDarkness());
                            }
                        }

                        var avg = (int) dark.Average();

                        if (avg >= minLevel)
                        {
                            result.Add(new ImageMineResponse(cellX, cellY, avg));
                        }
                    });
                }
            }
            #endregion

            return Results.Ok(new ImageRequestResponse(result));
        }
        catch (Exception ex)
        {
            return ErrorUnknown(ex.ToString());
        }
        finally
        {
            map?.Dispose();
        }
    }

    private static Image<Rgb24> Extract(Image<Rgb24> sourceImage, Rectangle sourceArea)
    {
        Image<Rgb24> targetImage = new(sourceArea.Width, sourceArea.Height);
        int height = sourceArea.Height;
        sourceImage.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
        {
            for (int i = 0; i < height; i++)
            {
                Span<Rgb24> sourceRow = sourceAccessor.GetRowSpan(sourceArea.Y + i);
                Span<Rgb24> targetRow = targetAccessor.GetRowSpan(i);

                sourceRow.Slice(sourceArea.X, sourceArea.Width).CopyTo(targetRow);
            }
        });

        return targetImage;
    }
}