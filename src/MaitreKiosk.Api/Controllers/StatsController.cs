using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaitreKiosk.Api.Controllers;

[ApiController]
[Route("api")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _stats;

    public StatsController(IStatsService stats) => _stats = stats;

    // GET /api/restaurants/1/top-products
    [HttpGet("restaurants/{restaurantId:int}/top-products")]
    public async Task<ActionResult<List<TopProductResponse>>> TopProducts(int restaurantId)
    {
        return Ok(await _stats.GetTopProductsAsync(restaurantId));
    }
}