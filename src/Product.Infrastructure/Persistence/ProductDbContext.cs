using Microsoft.EntityFrameworkCore;
using ProductEntity = Product.Domain.Entities.Product;
namespace Product.Infrastructure.Persistence;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<ProductEntity>();
        product.ToTable("products", table => {
            table.HasCheckConstraint("CK_products_price", "\"Price\" > 0");
            table.HasCheckConstraint("CK_products_stock", "\"Stock\" >= 0");
        });
        product.HasKey(x => x.Id);
        product.Property(x => x.Name).HasMaxLength(200).IsRequired();
        product.Property(x => x.Price).HasPrecision(12, 2);
    }
}
