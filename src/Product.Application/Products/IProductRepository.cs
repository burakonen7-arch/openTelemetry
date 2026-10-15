using ProductEntity = Product.Domain.Entities.Product;
namespace Product.Application.Products;
public interface IProductRepository
{
    Task<IReadOnlyList<ProductEntity>> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<ProductEntity?> FindAsync(Guid id, CancellationToken ct);
    void Add(ProductEntity product);
    void Remove(ProductEntity product);
    Task SaveAsync(CancellationToken ct);
}
