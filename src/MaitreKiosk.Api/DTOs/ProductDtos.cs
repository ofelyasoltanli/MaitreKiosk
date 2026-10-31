using System.ComponentModel.DataAnnotations;
using MaitreKiosk.Api.Domain;

namespace MaitreKiosk.Api.Dtos;

public class CreateProductRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }
}

public class UpdateProductRequest
{
    [Range(0.01, 100000)]
    public decimal? Price { get; set; }

    public bool? IsAvailable { get; set; }
}

public class ProductResponse
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }

    public static ProductResponse From(Product p) => new()
    {
        Id = p.Id,
        RestaurantId = p.RestaurantId,
        Name = p.Name,
        Category = p.Category,
        Price = p.Price,
        IsAvailable = p.IsAvailable
    };
}