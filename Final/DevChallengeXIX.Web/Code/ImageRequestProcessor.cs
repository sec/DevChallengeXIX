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

    public async Task<IResult> ProcessImage(int minLevel, string image, int Width = 0, int Height = 0)
    {
        byte[] bytes;
        int cellWidth = Width;
        int cellHeight = Height;
        Image<Rgb24>? map = null;
        List<Task<ImageMineResponse>> tasks = new();

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
                map.ProcessPixelRows(a =>
                {
                    for (var x = 1; x < map.Width; x++)
                    {
                        bool isBorder = true;
                        for (int y = 0; y < a.Height; y++)
                        {
                            var row = a.GetRowSpan(y);
                            if (!row[x].IsWhite())
                            {
                                isBorder = false;
                                break;
                            }
                        }

                        if (isBorder)
                        {
                            cellWidth = x - 1;
                            break;
                        }
                    }
                });

                if (cellWidth <= 0)
                {
                    return ErrorWidth();
                }
            }

            #endregion

            #region Find height of cell
            if (cellHeight <= 0)
            {
                map.ProcessPixelRows(a =>
                {
                    for (var y = 1; y < a.Height; y++)
                    {
                        bool isBorder = true;
                        var row = a.GetRowSpan(y);
                        for (var x = 0; x < a.Width; x++)
                        {
                            if (!row[x].IsWhite())
                            {
                                isBorder = false;
                                break;
                            }
                        }

                        if (isBorder)
                        {
                            cellHeight = y - 1;
                            break;
                        }
                    }
                });

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
                    var small = Extract(map, slice);
                    var cx = cellX;
                    var cy = cellY;

                    tasks.Add(Task.Run(() => Process(small, cx, cy)));
                }
            }
            #endregion

            await Task.WhenAll(tasks);

            return Results.Ok(new ImageRequestResponse(tasks.Where(x => x.Result.Level >= minLevel).Select(x => x.Result)));
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

    private static ImageMineResponse Process(Image<Rgb24> image, int cellX, int cellY)
    {
        var dark = new List<int>();
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    dark.Add(row[x].CalcDarkness());
                }
            }
        });

        image.Dispose();

        return new ImageMineResponse(cellX, cellY, (int) dark.Average());
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