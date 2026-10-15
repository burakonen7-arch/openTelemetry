using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Products;
namespace Product.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(
        [FromQuery, Range(1, 1000000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
        => Ok(await products.ListAsync(page, pageSize, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Get(Guid id, CancellationToken ct)
        => await products.GetAsync(id, ct) is { } product ? Ok(product) : NotFound();
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken ct)
    {
        var product = await products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, ProductRequest request, CancellationToken ct)
        => await products.UpdateAsync(id, request, ct) is { } product ? Ok(product) : NotFound();
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => await products.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
