using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CafeteriaPOS.Tests;

public class FluentAssertionsTests
{
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy_WithFluentAssertions()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task MissingOrder_ReturnsNotFound_WithFluentAssertions()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v2/orders/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record HealthResponse(string Status);
}
