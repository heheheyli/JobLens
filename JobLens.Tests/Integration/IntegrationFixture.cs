using JobLens.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace JobLens.Tests.Integration;

/// <summary>
/// Starts a real PostgreSQL container, then the whole web app on top of it.
/// The database connection comes from an environment variable, exactly as it does in
/// Azure. Only the Azure AI skill extractor is swapped for a fake.
/// </summary>
public class IntegrationFixture : IAsyncLifetime
{
    public const string FakeCommit = "0123456789abcdef-integration-test";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("BUILD_COMMIT", FakeCommit);
        Environment.SetEnvironmentVariable("BUILD_TIME", "integration-test");

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISkillExtractor>();
                services.AddScoped<ISkillExtractor, FakeSkillExtractor>();
            }));
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
    public const string Name = "Integration";
}
