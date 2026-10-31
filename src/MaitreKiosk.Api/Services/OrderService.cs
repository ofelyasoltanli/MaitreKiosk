using MaitreKiosk.Api.Data;
using MaitreKiosk.Api.Domain;
using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MaitreKiosk.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(int restaurantId, CreateOrderRequest request);
    Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request);
    Task<PagedResponse<OrderResponse>> ListAsync(int restaurantId, string? status, int page, int pageSize);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    // İcazə verilən keçidlər: hər status hansı statuslara keçə bilər
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending]   = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
        [OrderStatus.Preparing] = new[] { OrderStatus.Ready },
        [OrderStatus.Ready]     = new[] { OrderStatus.Completed },
        [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    public async Task<OrderResponse> CreateAsync(int restaurantId, CreateOrderRequest request)
    {
        var restaurantExists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new NotFoundException($"Restoran tapılmadı (id={restaurantId}).");

        // Eyni məhsul bir neçə sətirdə gəlibsə, saylarını birləşdiririk
        var merged = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        if (merged.Any(m => m.Quantity > 100))
            throw new BadRequestException("Bir məhsul üçün maksimum say 100-dür.");

        var productIds = merged.Select(m => m.ProductId).ToList();

        // Bütün məhsulları bir sorğu ilə gətiririk
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var order = new Order
        {
            RestaurantId = restaurantId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in merged)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new BadRequestException($"Məhsul tapılmadı (id={line.ProductId}).");

            if (product.RestaurantId != restaurantId)
                throw new BadRequestException($"Məhsul bu restorana aid deyil (id={line.ProductId}).");

            if (!product.IsAvailable)
                throw new BadRequestException($"Məhsul hazırda satışda deyil: {product.Name}.");

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,       // snapshot
                UnitPrice = product.Price,        // snapshot
                Quantity = line.Quantity,
                LineTotal = product.Price * line.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.LineTotal);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return OrderResponse.From(order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        var newStatus = ParseStatus(request.Status);

        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Sifariş tapılmadı (id={orderId}).");

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new ConflictException(
                $"Status {order.Status} → {newStatus} keçidi mümkün deyil.");

        order.Status = newStatus;
        await _db.SaveChangesAsync();

        return OrderResponse.From(order);
    }

    public async Task<PagedResponse<OrderResponse>> ListAsync(int restaurantId, string? status, int page, int pageSize)
    {
        if (page < 1)
            throw new BadRequestException("page ən azı 1 olmalıdır.");
        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException("pageSize 1 ilə 100 arasında olmalıdır.");

        var restaurantExists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new NotFoundException($"Restoran tapılmadı (id={restaurantId}).");

        var query = _db.Orders.AsNoTracking().Where(o => o.RestaurantId == restaurantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = ParseStatus(status);
            query = query.Where(o => o.Status == parsed);
        }

        var totalCount = await query.CountAsync();

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(OrderResponse.From).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static OrderStatus ParseStatus(string value)
    {
        // int.TryParse yoxlaması "1" kimi rəqəmlərin enum kimi qəbul olunmasının qarşısını alır
        if (int.TryParse(value, out _) || !Enum.TryParse<OrderStatus>(value, ignoreCase: true, out var status))
            throw new BadRequestException(
                $"Yanlış status: '{value}'. Mümkün dəyərlər: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
        return status;
    }
}