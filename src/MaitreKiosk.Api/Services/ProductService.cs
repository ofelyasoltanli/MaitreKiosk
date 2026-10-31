using MaitreKiosk.Api.Data;
using MaitreKiosk.Api.Domain;
using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MaitreKiosk.Api.Services;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(int restaurantId, CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int productId, UpdateProductRequest request);
    Task<ProductResponse> GetAsync(int productId);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<ProductResponse> CreateAsync(int restaurantId, CreateProductRequest request)
    {
        var restaurantExists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new NotFoundException($"Restoran tapılmadı (id={restaurantId}).");

        EnsureMaxTwoDecimals(request.Price);

        var product = new Product
        {
            RestaurantId = restaurantId,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Price = request.Price,
            IsAvailable = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return ProductResponse.From(product);
    }

    public async Task<ProductResponse> UpdateAsync(int productId, UpdateProductRequest request)
    {
        if (request.Price is null && request.IsAvailable is null)
            throw new BadRequestException("Ən azı bir sahə göndərilməlidir: price və ya isAvailable.");

        var product = await _db.Products.FindAsync(productId)
            ?? throw new NotFoundException($"Məhsul tapılmadı (id={productId}).");

        if (request.Price is not null)
        {
            EnsureMaxTwoDecimals(request.Price.Value);
            product.Price = request.Price.Value;
        }

        if (request.IsAvailable is not null)
            product.IsAvailable = request.IsAvailable.Value;

        await _db.SaveChangesAsync();
        return ProductResponse.From(product);
    }

    public async Task<ProductResponse> GetAsync(int productId)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException($"Mehsul tapılmadı (id={productId}).");

        return ProductResponse.From(product);
    }

    private static void EnsureMaxTwoDecimals(decimal price)
    {
        if (decimal.Round(price, 2) != price)
            throw new BadRequestException("Qiymet maksimum 2 onluq reqeme malik ola biler.");
    }
}