using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddScoped<IValidator<PriceUpdateRequest>, PriceUpdateValidator>();

builder.Services
    .AddAuthentication("DemoKey")
    .AddScheme<AuthenticationSchemeOptions, DemoKeyAuthenticationHandler>("DemoKey", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("price-updates", policy =>
    {
        policy.PermitLimit = 10;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueLimit = 0;
        policy.AutoReplenishment = true;
    });
});

var app = builder.Build();

// Swagger is intentionally restricted to Development (not Production).
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/", () => Results.Ok(new
{
    service = "Cafeteria POS API",
    version = "2.0",
    supportedVersions = new[] { "v1", "v2" }
}));

// Existing API endpoints remain unchanged for backwards compatibility.
app.MapGet("/api/v1/menu", (OrderService service) => Results.Ok(service.GetMenu()));
app.MapGet("/api/v1/orders", (OrderService service) => Results.Ok(service.GetOrders()));
app.MapPost("/api/v1/orders", (CreateOrderRequest request, OrderService service) =>
    service.CreateOrder(request));
app.MapGet("/api/v2/orders", (OrderService service) => Results.Ok(service.GetOrdersV2()));
app.MapGet("/api/v2/orders/{id:int}", (int id, OrderService service) =>
    service.GetOrderV2(id));

// PR8: only Admin can change a menu price. Missing key => 401; Cashier => 403.
app.MapPut("/api/v2/menu/{id:int}/price",
    async (int id, PriceUpdateRequest request,
           IValidator<PriceUpdateRequest> validator, OrderService service) =>
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());
        return service.UpdatePrice(id, request.Price);
    })
    .RequireAuthorization("AdminOnly")
    .RequireRateLimiting("price-updates");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.Run();

record MenuItem(int Id, string Name, decimal Price, bool IsAvailable);
record OrderLine(int MenuItemId, string Name, decimal UnitPrice, int Quantity);
record Order(int Id, DateTime CreatedAtUtc, List<OrderLine> Items, decimal Total, string Status);
record OrderItemRequest(int MenuItemId, int Quantity);
record CreateOrderRequest(List<OrderItemRequest> Items);
record PriceUpdateRequest(decimal Price);
public partial class Program { }
