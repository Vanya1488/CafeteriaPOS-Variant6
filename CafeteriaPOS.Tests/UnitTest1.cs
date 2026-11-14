
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CafeteriaPOS.Tests;

public class CafeteriaApiTests
{
    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>();
    }

    // ПР1. Проверка получения меню
    [Fact]
    public async Task GetMenu_ReturnsThreeItems()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/menu");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(3, json.RootElement.GetArrayLength());
    }

    // ПР2. Проверка получения списка заказов
    [Fact]
    public async Task GetOrders_ReturnsEmptyList()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(0, json.RootElement.GetArrayLength());
    }

    // ПР3. Проверка создания корректного заказа
    [Fact]
    public async Task CreateOrder_ValidRequest_Returns201()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            items = new[]
            {
                new { menuItemId = 1, quantity = 2 },
                new { menuItemId = 2, quantity = 1 }
            }
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(1, json.RootElement.GetProperty("id").GetInt32());
        Assert.Equal(490m, json.RootElement.GetProperty("total").GetDecimal());
    }

    // ПР4. Проверка отклонения пустого заказа
    [Fact]
    public async Task CreateOrder_EmptyItems_Returns400()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new { items = Array.Empty<object>() };

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ПР5. Проверка недоступного блюда
    [Fact]
    public async Task CreateOrder_UnavailableItem_Returns400()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            items = new[]
            {
                new { menuItemId = 999, quantity = 1 }
            }
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ПР6. Проверка получения существующего заказа через V2
    [Fact]
    public async Task GetOrderV2_ExistingOrder_Returns200()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            items = new[]
            {
                new { menuItemId = 1, quantity = 2 }
            }
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/orders", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        int id = createdJson.RootElement
            .GetProperty("id").GetInt32();

        var response = await client.GetAsync(
            $"/api/v2/orders/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(2, json.RootElement.GetProperty("itemCount").GetInt32());
        Assert.Equal("2.0", json.RootElement.GetProperty("apiVersion").GetString());
    }

    // ПР7. Проверка отсутствующего заказа
    [Fact]
    public async Task GetOrderV2_MissingOrder_Returns404()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v2/orders/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ПР8. Проверка работоспособности приложения
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "Healthy",
            json.RootElement.GetProperty("status").GetString());
    }
}
