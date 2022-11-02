using DevChallengeXIX.Database;
using DevChallengeXIX.Requests;
using DevChallengeXIX.Web;
using Microsoft.EntityFrameworkCore;

namespace DevChallengeXIX;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<TrustContext>(o => o.UseSqlite("Filename=trust.sdb"));
        builder.Services.AddScoped<ITrustContext, TrustContext>();
        builder.Services.AddScoped<TrustService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            using var context = scope.ServiceProvider.GetRequiredService<TrustContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        app.UseHttpsRedirection();

        app.MapPost("/api/people", (PersonRequest request, TrustService ts) => ts.CreatePerson(request));

        app.MapPost("/api/people/{id}/trust_connections", (string id, IDictionary<string, int> request, TrustService ts) => ts.AddOrUpdateTrust(id, request));

        app.MapPost("/api/messages", (MessageRequest request, TrustService ts) => ts.SendMessage(request));

        app.MapPost("/api/path", (MessageRequest request, TrustService ts) => ts.SendPath(request));

        app.Run();
    }
}