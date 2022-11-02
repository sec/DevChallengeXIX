# DevChallenge XIX Final

## Local build/run/test

1. Install or make sure you have already installed .NET SDK 6
1. `dotnet build`
1. `dotnet test`
1. `dotnet run --project DevChallengeXIX.Web`
1. Use `http://localhost:8080` or `https://localhost:8081`
1. There's Swagger available at `/swagger` endpoint - it's also available under Docker for easy testing (it should be disabled of course for production release)

## Docker

1. `docker compose build`
1. `docker compose up`
1. Use `http://localhost:8080` to send requests to the API

## Background

Endpoint `api/image-input` can take this payload

```
{
  "min_level": 0,
  "image": "string",
  "cellWidth": 0,
  "cellHeight": 0
}
```

- Note that `cellWidth` and `cellHeight` are *optional* - their default value is 0 - those can be ommited when sending request.
- If you don't send them - API will try to detect single cell width and height based on white border.
- If you send width or height (or both) - then logic for given dimension will be skipped.

## How calculation is done
1. Determine cell width and height
1. For each pixel in cell, calculate `darkness` using formula `(1 - pixel / 255) * 100` and round to integer - this will be in `0-100` range
1. Darkness for black pixel RGB(0, 0, 0) = 
1. Store that value for each cell using correct X,Y
1. When looking for cells with given `min_level` calculate mean/average for each cell and return those `>= min_level` - as we already have `0-100` ranges of darkness calculated

### Important changes
1. When image is not perfect square and the edges (just like in the examples provided) - the code will work and calculate darkness for those areas also.
1. Extra parameters `cellWidth` and `cellHeight` are not required - those are added as bonus value, for images that can't have grid autodetected for some reason (ex. all white pixels in the first rows, columns, etc.)