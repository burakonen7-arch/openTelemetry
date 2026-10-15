using Microsoft.EntityFrameworkCore;
using Product.Application.Products;
using ProductEntity = Product.Domain.Entities.Product;
namespace Product.Infrastructure.Persistence;
public sealed class ProductRepository(ProductDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<ProductEntity>> ListAsync(int page, int pageSize, CancellationToken ct)
        => await db.Products.AsNoTracking().OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    public Task<ProductEntity?> FindAsync(Guid id, CancellationToken ct) => db.Products.SingleOrDefaultAsync(x => x.Id == id, ct);
    public void Add(ProductEntity product) => db.Products.Add(product);
    public void Remove(ProductEntity product) => db.Products.Remove(product);
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
