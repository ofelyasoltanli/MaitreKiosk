using MaitreKiosk.Api.Dtos;
using MaitreKiosk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaitreKiosk.Api.Controllers;

[ApiController]
[Route("api")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    // POST /api/restaurants/1/products
    [HttpPost("restaurants/{restaurantId:int}/products")]
    public async Task<ActionResult<ProductResponse>> Create(int restaurantId, CreateProductRequest request)
    {
        var created = await _products.CreateAsync(restaurantId, request);
        return CreatedAtAction(nameof(Get), new { productId = created.Id }, created);
    }

    // PATCH /api/products/5
    [HttpPatch("products/{productId:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int productId, UpdateProductRequest request)
    {
        return Ok(await _products.UpdateAsync(productId, request));
    }

    // GET /api/products/5
    [HttpGet("products/{productId:int}")]
    public async Task<ActionResult<ProductResponse>> Get(int productId)
    {
        return Ok(await _products.GetAsync(productId));
    }
}