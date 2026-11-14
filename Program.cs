var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

// Сохраняем поведение существующего учебного прототипа.
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new
{
    service = "Cafeteria POS API",
    version = "2.0",
    supportedVersions = new[] { "v1", "v2" }
}));

// API V1: существующие контракты не меняются.
app.MapGet("/api/v1/menu", (OrderService service) => Results.Ok(service.GetMenu()));
app.MapGet("/api/v1/orders", (OrderService service) => Results.Ok(service.GetOrders()));
app.MapPost("/api/v1/orders", (CreateOrderRequest request, OrderService service) =>
    service.CreateOrder(request));

// API V2: поля ItemCount и ApiVersion сохранены.
app.MapGet("/api/v2/orders", (OrderService service) => Results.Ok(service.GetOrdersV2()));
app.MapGet("/api/v2/orders/{id:int}", (int id, OrderService service) =>
    service.GetOrderV2(id));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

record MenuItem(int Id, string Name, decimal Price, bool IsAvailable);
record OrderLine(int MenuItemId, string Name, decimal UnitPrice, int Quantity);
record Order(int Id, DateTime CreatedAtUtc, List<OrderLine> Items, decimal Total, string Status);
record OrderItemRequest(int MenuItemId, int Quantity);
record CreateOrderRequest(List<OrderItemRequest> Items);

// Используется WebApplicationFactory<Program> в интеграционных тестах.
public partial class Program { }
