using DevChallengeXIX.Web.Code;
using DevChallengeXIX.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DevChallengeXIX.Web;

public class Program
{
    public const string ImageInputEndpoint = "/api/image-input";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<ImageRequestProcessor>();

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapPost(ImageInputEndpoint, PostImage);
        app.MapGet("/healthcheck", () => "OK");
        app.Run();
    }

    [ProducesResponseType(typeof(ImageRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ImageErrorReposne), StatusCodes.Status422UnprocessableEntity)]
    static Task<IResult> PostImage(ImageRequest r, ImageRequestProcessor p) => p.ProcessImage(r.Min_level, r.Image, r.CellWidth, r.CellHeight);
}