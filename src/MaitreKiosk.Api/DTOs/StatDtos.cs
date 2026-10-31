namespace MaitreKiosk.Api.Dtos;

public class TopProductResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}