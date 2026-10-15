using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Product.Application.Products;
using ProductEntity = Product.Domain.Entities.Product;
namespace Product.Application.Products;

public sealed class ProductService(IProductRepository db, ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        using var activity = ProductTelemetry.Activities.StartActivity("products.list");
        return (await db.ListAsync(page, pageSize, ct)).Select(Map).ToList();
    }
    public async Task<ProductResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        using var activity = ProductTelemetry.Activities.StartActivity("products.get");
        activity?.SetTag("product.id", id);
        var product = await db.FindAsync(id, ct);
        activity?.SetTag("product.found", product is not null);
        return product is null ? null : Map(product);
    }
    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct)
    {
        using var activity = ProductTelemetry.Activities.StartActivity("products.create");
        var product = new ProductEntity { Name = request.Name.Trim(), Price = request.Price, Stock = request.Stock };
        db.Add(product);
        await db.SaveAsync(ct);
        ProductTelemetry.Created.Add(1);
        activity?.SetTag("product.id", product.Id);
        activity?.AddEvent(new ActivityEvent("product.persisted"));
        logger.LogInformation("Product {ProductId} created", product.Id);
        return Map(product);
    }
    public async Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct)
    {
        using var activity = ProductTelemetry.Activities.StartActivity("products.update");
        var product = await db.FindAsync(id, ct);
        if (product is null) return null;
        product.Name = request.Name.Trim(); product.Price = request.Price; product.Stock = request.Stock;
        await db.SaveAsync(ct);
        logger.LogInformation("Product {ProductId} updated", id);
        return Map(product);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        using var activity = ProductTelemetry.Activities.StartActivity("products.delete");
        var product = await db.FindAsync(id, ct);
        if (product is null) return false;
        db.Remove(product);
        await db.SaveAsync(ct);
        logger.LogInformation("Product {ProductId} deleted", id);
        return true;
    }
    private static ProductResponse Map(ProductEntity x) => new(x.Id, x.Name, x.Price, x.Stock, x.CreatedAt);
}
