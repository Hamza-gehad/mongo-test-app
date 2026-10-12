using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MongoTest.ApiService.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string TestDatabaseName { get; } = $"MongoTestDb_{Guid.NewGuid():N}";

    public MongoClient MongoClient { get; }

    public CustomWebApplicationFactory()
    {
        MongoClient = new MongoClient("mongodb://localhost:27017");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("MongoDbSettings:ConnectionString", "mongodb://localhost:27017");
        builder.UseSetting("MongoDbSettings:DatabaseName", TestDatabaseName);
    }

    /// <summary>Drops the entire test database to clean up between runs.</summary>
    public void DropTestDatabase()
    {
        MongoClient.DropDatabase(TestDatabaseName);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            MongoClient.DropDatabase(TestDatabaseName);
        }
        base.Dispose(disposing);
    }
}