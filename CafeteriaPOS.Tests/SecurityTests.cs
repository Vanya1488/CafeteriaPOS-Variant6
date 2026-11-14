using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CafeteriaPOS.Tests;

public class SecurityTests
{
    private static WebApplicationFactory<Program> Factory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DemoAuth:AdminKey"] = "test-admin-secret-not-for-production",
                    ["DemoAuth:CashierKey"] = "test-cashier-secret-not-for-production"
                }));
        });

    [Fact]
    public async Task PriceUpdate_WithoutKey_Returns401()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PutAsJsonAsync("/api/v2/menu/1/price", new { price = 300 });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PriceUpdate_WrongKey_Returns401()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Key", "invalid-key");
        var response = await client.PutAsJsonAsync("/api/v2/menu/1/price", new { price = 300 });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PriceUpdate_CashierKey_Returns403()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Key", "test-cashier-secret-not-for-production");
        var response = await client.PutAsJsonAsync("/api/v2/menu/1/price", new { price = 300 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PriceUpdate_AdminKey_Returns204AndUpdatesMenu()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Key", "test-admin-secret-not-for-production");
        var response = await client.PutAsJsonAsync("/api/v2/menu/1/price", new { price = 300 });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var menu = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/v1/menu");
        menu[0].GetProperty("price").GetDecimal().Should().Be(300m);
    }

    [Fact]
    public async Task PriceUpdate_InvalidPrice_Returns400()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Key", "test-admin-secret-not-for-production");
        var response = await client.PutAsJsonAsync("/api/v2/menu/1/price", new { price = -1 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
