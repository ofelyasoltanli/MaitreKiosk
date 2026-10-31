using System.ComponentModel.DataAnnotations;
using MaitreKiosk.Api.Domain;

namespace MaitreKiosk.Api.Dtos;

public class CreateOrderRequest
{
    [Required, MinLength(1), MaxLength(50)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class OrderItemResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderResponse
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();

    public static OrderResponse From(Order o) => new()
    {
        Id = o.Id,
        RestaurantId = o.RestaurantId,
        Status = o.Status.ToString(),
        TotalAmount = o.TotalAmount,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.LineTotal
        }).ToList()
    };
}