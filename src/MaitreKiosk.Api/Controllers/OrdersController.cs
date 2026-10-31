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

    // PATCH /api/orders/5/status
    [HttpPatch("orders/{orderId:int}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int orderId, UpdateOrderStatusRequest request)
    {
        return Ok(await _orders.UpdateStatusAsync(orderId, request));
    }

    // GET /api/restaurants/1/orders?status=Pending&page=1&pageSize=20
    [HttpGet("restaurants/{restaurantId:int}/orders")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> List(
        int restaurantId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _orders.ListAsync(restaurantId, status, page, pageSize));
    }
}