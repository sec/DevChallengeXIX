using DevChallengeXIX.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace DevChallengeXIX.Tests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    SqliteConnection? _sqliteConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";
        _sqliteConnection = new SqliteConnection(connectionString);
        _sqliteConnection.Open();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TrustContext>));
            ArgumentNullException.ThrowIfNull(descriptor);

            services.Remove(descriptor);
            services.AddDbContext<TrustContext>(options =>
            {

                options.UseSqlite(connectionString);

            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _sqliteConnection?.Close();
            _sqliteConnection?.Dispose();
        }
    }
}