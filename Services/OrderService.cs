// Сервис выделяет бизнес-логику из Program.cs.
// Хранение остаётся учебным: в памяти текущего процесса.
internal sealed class OrderService
{
    private readonly object _sync = new();
    private readonly List<MenuItem> _menu = new()
    {
        new(1, "Суп дня", 120m, true),
        new(2, "Курица с рисом", 250m, true),
        new(3, "Чай", 50m, true)
    };

    private readonly List<Order> _orders = new();

    public List<MenuItem> GetMenu() => _menu.ToList();

    public List<Order> GetOrders()
    {
        lock (_sync)
            return _orders.ToList();
    }

    public IResult CreateOrder(CreateOrderRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            return Results.BadRequest(new { error = "Заказ должен содержать позиции." });

        var lines = new List<OrderLine>();
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return Results.BadRequest(new { error = "Количество должно быть положительным." });

            var product = _menu.FirstOrDefault(x => x.Id == item.MenuItemId && x.IsAvailable);
            if (product is null)
                return Results.BadRequest(new { error = $"Позиция {item.MenuItemId} недоступна." });

            lines.Add(new OrderLine(product.Id, product.Name, product.Price, item.Quantity));
        }

        decimal total = lines.Sum(x => x.UnitPrice * x.Quantity);
        Order order;
        lock (_sync)
        {
            order = new Order(_orders.Count + 1, DateTime.UtcNow, lines, total, "Accepted");
            _orders.Add(order);
        }

        return Results.Created($"/api/v1/orders/{order.Id}", order);
    }

    public object[] GetOrdersV2()
    {
        lock (_sync)
            return _orders.Select(ToV2).ToArray();
    }

    public IResult GetOrderV2(int id)
    {
        lock (_sync)
        {
            var order = _orders.FirstOrDefault(x => x.Id == id);
            return order is null
                ? Results.NotFound(new { error = "Заказ не найден." })
                : Results.Ok(ToV2(order));
        }
    }

    private static object ToV2(Order order) => new
    {
        order.Id,
        order.CreatedAtUtc,
        order.Items,
        order.Total,
        order.Status,
        ItemCount = order.Items.Sum(x => x.Quantity),
        ApiVersion = "2.0"
    };
}
