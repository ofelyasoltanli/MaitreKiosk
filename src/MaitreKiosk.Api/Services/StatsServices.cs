using MaitreKiosk.Api.Data;
using MaitreKiosk.Api.Domain;
using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MaitreKiosk.Api.Services;

public interface IStatsService
{
    Task<List<TopProductResponse>> GetTopProductsAsync(int restaurantId);
}

public class StatsService : IStatsService
{
    private readonly AppDbContext _db;

    public StatsService(AppDbContext db) => _db = db;

    public async Task<List<TopProductResponse>> GetTopProductsAsync(int restaurantId)
    {
        var restaurantExists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new NotFoundException($"Restoran tapılmadı (id={restaurantId}).");

        return await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.RestaurantId == restaurantId
                     && i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductResponse
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalQuantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenBy(x => x.ProductName)
            .Take(5)
            .ToListAsync();
    }
}