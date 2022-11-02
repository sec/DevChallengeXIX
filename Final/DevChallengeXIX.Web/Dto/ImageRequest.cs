namespace DevChallengeXIX.Web.Dto;

public record ImageRequest(int Min_level, string Image, int CellWidth = 0, int CellHeight = 0);

public record ImageRequestResponse(IEnumerable<ImageMineResponse> Mines);

public record ImageMineResponse(int X, int Y, int Level);

public record ImageErrorReposne(string Error, string? Details = null);