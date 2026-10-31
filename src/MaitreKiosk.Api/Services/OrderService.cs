using MaitreKiosk.Api.Data;
using MaitreKiosk.Api.Domain;
using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MaitreKiosk.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(int restaurantId, CreateOrderRequest request);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

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
}