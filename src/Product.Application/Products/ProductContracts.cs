using System.ComponentModel.DataAnnotations;
namespace Product.Application.Products;

public sealed record ProductRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Range(typeof(decimal), "0.01", "9999999999.99")] decimal Price,
    [Range(0, int.MaxValue)] int Stock);
public sealed record ProductResponse(Guid Id, string Name, decimal Price, int Stock, DateTimeOffset CreatedAt);

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<ProductResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct);
    Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
