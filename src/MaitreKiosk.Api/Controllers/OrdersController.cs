using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaitreKiosk.Api.Controllers;

[ApiController]
[Route("api")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    // POST /api/restaurants/1/orders
    [HttpPost("restaurants/{restaurantId:int}/orders")]
    public async Task<ActionResult<OrderResponse>> Create(int restaurantId, CreateOrderRequest request)
    {
        var created = await _orders.CreateAsync(restaurantId, request);
        return StatusCode(StatusCodes.Status201Created, created);
    }
}