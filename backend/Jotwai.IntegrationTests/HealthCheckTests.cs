using System.Net;

namespace Jotwai.IntegrationTests;

public sealed class HealthCheckTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;

    public HealthCheckTests(JotwaiApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_endpoint_returns_ok_when_database_is_reachable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
